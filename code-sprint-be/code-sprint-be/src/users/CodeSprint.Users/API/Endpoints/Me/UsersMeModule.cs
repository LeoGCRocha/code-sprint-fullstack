using CodeSprint.Users.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Users.API.Endpoints.Me;

/// <summary>
/// Vertical slice for <c>GET /users/me</c>. Returns the authenticated user's
/// profile. The caller is identified by the <c>sub</c> claim on the validated
/// JWT; <see cref="UserSyncMiddleware"/> guarantees the record exists by the
/// time this handler runs.
/// </summary>
public static class UsersMeModule
{
    public static IEndpointRouteBuilder MapUsersMe(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me", Handle)
            .RequireAuthorization()
            .WithName("GetCurrentUser");

        return app;
    }

    private static async Task<IResult> Handle(HttpContext http, UsersDbContext db)
    {
        var sub = http.User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub))
            return Results.Unauthorized();

        var parts = sub.Split('|', 2);
        if (parts.Length != 2)
            return Results.Unauthorized();

        var provider = parts[0];
        var providerSubject = parts[1];

        var user = await db.Users
            .AsNoTracking()
            .Where(u => u.Identity.Provider == provider && u.Identity.ProviderSubject == providerSubject)
            .Select(u => new UsersMeResponse(
                u.Id.ToString(),
                u.Handle.Value,
                u.DisplayName,
                u.Email.Address,
                u.Avatar,
                0,
                u.MemberSince))
            .FirstOrDefaultAsync();

        return user is null
            ? Results.NotFound()
            : Results.Ok(user);
    }
}
