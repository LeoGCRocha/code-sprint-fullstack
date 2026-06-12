using CodeSprint.Users.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Users.API.Endpoints.Me;

/// <summary>
/// Vertical slice for <c>GET /users/me/submission-activity</c>. Returns the
/// authenticated user's per-day activity counts (the heatmap read model), shaped as
/// a <c>{ "yyyy-MM-dd": count }</c> map for the profile heatmap. Read-only query
/// over the <c>heatmap_days</c> projection — no domain logic.
/// </summary>
/// <remarks>
/// The caller is identified by the <c>sub</c> claim (provider|subject), the same
/// resolution <see cref="UsersMeModule"/> uses. Optional <c>from</c>/<c>to</c> query
/// parameters (ISO dates) bound the window; the default is the trailing 12 months.
/// </remarks>
public static class UsersMeSubmissionActivityModule
{
    public static IEndpointRouteBuilder MapUsersMeSubmissionActivity(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me/submission-activity", Handle)
            .RequireAuthorization()
            .WithName("GetCurrentUserSubmissionActivity");

        return app;
    }

    private static async Task<IResult> Handle(HttpContext http, UsersDbContext db, DateOnly? from, DateOnly? to)
    {
        var sub = http.User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub))
            return Results.Unauthorized();

        var parts = sub.Split('|', 2);
        if (parts.Length != 2)
            return Results.Unauthorized();

        var provider = parts[0];
        var providerSubject = parts[1];

        var userId = await db.Users
            .AsNoTracking()
            .Where(u => u.Identity.Provider == provider && u.Identity.ProviderSubject == providerSubject)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        if (userId == default)
            return Results.NotFound();

        // Default window: trailing 12 months, in UTC days (matches the projection's bucket).
        var until = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var since = from ?? until.AddDays(-365);

        var activity = await db.HeatmapDays
            .AsNoTracking()
            .Where(h => h.UserId == userId && h.Day >= since && h.Day <= until)
            .OrderBy(h => h.Day)
            .Select(h => new { h.Day, h.Count })
            .ToDictionaryAsync(h => h.Day.ToString("yyyy-MM-dd"), h => h.Count);

        return Results.Ok(activity);
    }
}
