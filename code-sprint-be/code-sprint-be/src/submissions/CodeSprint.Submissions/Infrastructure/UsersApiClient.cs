using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeSprint.Shared.Ids;
using Microsoft.AspNetCore.Http;

namespace CodeSprint.Submissions.Infrastructure;

/// <summary>
/// Resolves the caller's Users-BC <see cref="UserId"/>.
///
/// The JWT <c>sub</c> claim is <c>provider|subject</c>, NOT the Users-BC id, so we
/// must ask the Users service who the caller is. This calls
/// <c>GET http://users-api/users/me</c> forwarding the inbound Authorization header;
/// Aspire service discovery resolves the <c>users-api</c> host. The Users service
/// maps the <c>sub</c> claim to its <c>UserId</c> and returns it as <c>id</c>.
/// </summary>
public sealed class UsersApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Returns the caller's <see cref="UserId"/>, or <c>null</c> when the Users
    /// service could not be reached / returned a non-success status / an
    /// unparseable id. The endpoint translates <c>null</c> into the right HTTP code.
    /// </summary>
    public async Task<UserId?> GetCurrentUserIdAsync(CancellationToken ct = default)
    {
        var token = ExtractBearer(httpContextAccessor.HttpContext);
        if (string.IsNullOrWhiteSpace(token))
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, "/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var me = await JsonSerializer.DeserializeAsync<MeResponse>(stream, JsonOpts, ct);

        if (me?.Id is null || !Guid.TryParse(me.Id, out var guid))
            return null;

        return UserId.From(guid);
    }

    private static string? ExtractBearer(HttpContext? httpContext)
    {
        if (httpContext is null)
            return null;

        var header = httpContext.Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..].Trim()
            : null;
    }

    private sealed record MeResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }
    }
}
