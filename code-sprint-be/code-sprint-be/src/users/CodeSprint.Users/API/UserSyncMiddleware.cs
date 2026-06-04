using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using CodeSprint.Users.Domain;
using CodeSprint.Users.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Users.API;

/// <summary>
/// Ensures every authenticated caller has a corresponding <see cref="User"/>
/// record. Runs after authentication on each request; the fast path is a single
/// indexed existence check. On first sight of a user, profile data (email, name,
/// picture) is fetched from the provider's <c>/userinfo</c> endpoint using the
/// caller's access token, which carries the userinfo audience.
/// </summary>
public class UserSyncMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, UsersDbContext db, IHttpClientFactory httpClientFactory)
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            await next(httpContext);
            return;
        }

        var sub = httpContext.User.FindFirst("sub")?.Value;
        var parts = sub?.Split('|', 2);

        if (parts is not { Length: 2 })
        {
            await next(httpContext);
            return;
        }

        var provider = parts[0];
        var providerSubject = parts[1];

        var exists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Identity.Provider == provider && u.Identity.ProviderSubject == providerSubject);

        if (!exists)
        {
            await CreateUser(httpContext, db, httpClientFactory, provider, providerSubject);
        }

        await next(httpContext);
    }

    private static async Task CreateUser(
        HttpContext httpContext,
        UsersDbContext db,
        IHttpClientFactory httpClientFactory,
        string provider,
        string providerSubject)
    {
        var profile = await FetchUserInfo(httpContext, httpClientFactory);
        if (profile is null)
        {
            Console.WriteLine("[UserSync] skip create: could not fetch userinfo");
            return;
        }

        var emailResult = Email.Create(profile.Email);
        if (emailResult.IsFailure)
        {
            Console.WriteLine($"[UserSync] skip create: invalid email '{profile.Email}'");
            return;
        }

        var displayName = !string.IsNullOrWhiteSpace(profile.Name)
            ? profile.Name
            : emailResult.Value.LocalPart;

        var handle = await GenerateUniqueHandle(db, profile.Name ?? emailResult.Value.LocalPart);

        var user = User.Register(emailResult.Value, handle, displayName, profile.Picture, provider, providerSubject);

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Concurrent first request for the same user can race the existence
            // check; the unique index on (provider, provider_subject) absorbs it.
            Console.WriteLine($"[UserSync] create raced/failed: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    private static async Task<UserInfo?> FetchUserInfo(HttpContext httpContext, IHttpClientFactory httpClientFactory)
    {
        var issuer = httpContext.User.FindFirst("iss")?.Value;
        var token = ExtractBearer(httpContext);

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(token))
            return null;

        var client = httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"{issuer.TrimEnd('/')}/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[UserSync] userinfo failed: {(int)response.StatusCode}");
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<UserInfo>(stream);
    }

    private static string? ExtractBearer(HttpContext httpContext)
    {
        var header = httpContext.Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..].Trim()
            : null;
    }

    private static async Task<Handle> GenerateUniqueHandle(UsersDbContext db, string seed)
    {
        var sanitized = Regex.Replace(seed.ToLowerInvariant(), "[^a-z0-9_]", "");
        if (sanitized.Length < 3) sanitized = $"user{sanitized}";
        if (sanitized.Length > 14) sanitized = sanitized[..14];

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = attempt == 0
                ? sanitized
                : $"{sanitized}_{Random.Shared.Next(1000, 9999)}";

            var result = Handle.Create(candidate);
            if (result.IsFailure) continue;

            var taken = await db.Users.AnyAsync(u => u.Handle == result.Value);
            if (!taken) return result.Value;
        }

        // Last resort: fully random handle.
        return Handle.Create($"user_{Random.Shared.Next(100000, 999999)}").Value;
    }

    private sealed record UserInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("email")]
        public string? Email { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("picture")]
        public string? Picture { get; init; }
    }
}
