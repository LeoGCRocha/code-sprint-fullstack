using CodeSprint.Shared.Ids;
using CodeSprint.Submissions.Domain;
using CodeSprint.Submissions.Infrastructure;
using CodeSprint.Submissions.Infrastructure.Judge;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Submissions.API.Endpoints.Submissions;

/// <summary>
/// Vertical slice for the Submissions BC. Routes are INTERNAL paths (no <c>/api</c>
/// prefix); the gateway prefixes/strips <c>/api</c> at the edge.
/// </summary>
public static class SubmissionsModule
{
    public static IEndpointRouteBuilder MapSubmissions(this IEndpointRouteBuilder app)
    {
        app.MapPost("/submissions", Create).RequireAuthorization();
        app.MapGet("/submissions/{id}", GetById).RequireAuthorization();
        app.MapGet("/users/{userId}/submissions", ListForUser).RequireAuthorization();
        app.MapGet("/users/{userId}/submission-activity", GetActivity).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> Create(
        CreateSubmissionRequest req,
        SubmissionsDbContext db,
        UsersApiClient usersApi,
        IJudgeQueue queue,
        CancellationToken ct)
    {
        // The caller's Users-BC id is resolved cross-context (sub claim != UserId).
        var userId = await usersApi.GetCurrentUserIdAsync(ct);
        if (userId is null)
            return Results.StatusCode(StatusCodes.Status502BadGateway);

        if (!Guid.TryParse(req.ProblemId, out var problemGuid))
            return Results.BadRequest("Invalid problemId; expected a GUID");
        var problemId = ProblemId.From(problemGuid);

        if (!Enum.TryParse<Language>(req.Language, ignoreCase: true, out var language))
            return Results.BadRequest(
                $"Invalid language '{req.Language}'. Valid: {string.Join(", ", Enum.GetNames<Language>())}");

        var sourceCodeResult = SourceCode.Create(req.SourceCode);
        if (sourceCodeResult.IsFailure)
            return Results.BadRequest(sourceCodeResult.Error.Message);

        var submissionResult = Submission.Create(userId.Value, problemId, language, sourceCodeResult.Value);
        if (submissionResult.IsFailure)
            return ToProblem(submissionResult.Error);

        var submission = submissionResult.Value;
        db.Submissions.Add(submission);
        await db.SaveChangesAsync(ct);

        // Hand off to the in-process judge worker (Fase 2: durable transport).
        await queue.EnqueueAsync(submission.Id, ct);

        return Results.Accepted(
            $"/submissions/{submission.Id.Value}",
            new CreateSubmissionResponse(submission.Id.ToString(), "pending"));
    }

    private static async Task<IResult> GetById(string id, SubmissionsDbContext db, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var guid))
            return Results.NotFound();

        var submissionId = SubmissionId.From(guid);
        var submission = await db.Submissions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == submissionId, ct);

        return submission is null
            ? Results.NotFound()
            : Results.Ok(ToResponse(submission));
    }

    private static async Task<IResult> ListForUser(
        string userId,
        SubmissionsDbContext db,
        CancellationToken ct,
        int page = 1,
        int pageSize = 20)
    {
        if (!Guid.TryParse(userId, out var guid))
            return Results.BadRequest("Invalid userId; expected a GUID");

        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var uid = UserId.From(guid);
        var query = db.Submissions.AsNoTracking().Where(s => s.UserId == uid);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Results.Ok(new PagedResponse<SubmissionSummaryResponse>(
            items.Select(ToSummary).ToList(), total, page, pageSize));
    }

    // NOTE (Fase 1): the heatmap is computed on demand here. The documented target
    // is a Users-BC HEATMAP_DAY read model fed by SubmissionEvaluated events (Fase 2);
    // this endpoint stands in until that read model exists.
    private static async Task<IResult> GetActivity(string userId, SubmissionsDbContext db, CancellationToken ct)
    {
        if (!Guid.TryParse(userId, out var guid))
            return Results.BadRequest("Invalid userId; expected a GUID");

        var uid = UserId.From(guid);
        var since = DateTimeOffset.UtcNow.Date.AddDays(-365);

        var rows = await db.Submissions
            .AsNoTracking()
            .Where(s => s.UserId == uid && s.SubmittedAt >= since)
            .Select(s => s.SubmittedAt)
            .ToListAsync(ct);

        // date (yyyy-MM-dd) -> count; serialized as a JSON object for the frontend
        // Heatmap, which consumes Record<string, number>.
        var activity = rows
            .GroupBy(d => d.UtcDateTime.ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.Count());

        return Results.Ok(activity);
    }

    private static SubmissionResponse ToResponse(Submission s) => new(
        Id: s.Id.ToString(),
        ProblemId: s.ProblemId.ToString(),
        Language: s.Language.ToString(),
        Status: s.Status.ToString().ToLowerInvariant(),
        SubmittedAt: s.SubmittedAt,
        Evaluation: s.Evaluation is null ? null : new EvaluationResponse(
            Verdict: s.Evaluation.Verdict.ToString(),
            PointsAwarded: s.Evaluation.PointsAwarded,
            RuntimeMs: s.Evaluation.RuntimeMs,
            MemoryKb: s.Evaluation.MemoryKb,
            EvaluatedAt: s.Evaluation.EvaluatedAt,
            Results: s.Evaluation.Results
                .OrderBy(r => r.Ordinal)
                .Select(r => new TestCaseResultResponse(
                    Ordinal: r.Ordinal,
                    Status: r.Status.ToString(),
                    RuntimeMs: r.RuntimeMs,
                    MemoryKb: r.MemoryKb,
                    IsHidden: r.IsHidden,
                    // Redact: never expose hidden-case output (already null in storage).
                    ActualOutput: r.IsHidden ? null : r.ActualOutput))
                .ToList()));

    private static SubmissionSummaryResponse ToSummary(Submission s) => new(
        Id: s.Id.ToString(),
        ProblemId: s.ProblemId.ToString(),
        Language: s.Language.ToString(),
        Status: s.Status.ToString().ToLowerInvariant(),
        SubmittedAt: s.SubmittedAt,
        Verdict: s.Evaluation?.Verdict.ToString(),
        PointsAwarded: s.Evaluation?.PointsAwarded);

    private static IResult ToProblem(CodeSprint.Shared.Primitives.Error error) => error.Type switch
    {
        CodeSprint.Shared.Primitives.ErrorType.Validation => Results.BadRequest(error.Message),
        CodeSprint.Shared.Primitives.ErrorType.NotFound => Results.NotFound(error.Message),
        CodeSprint.Shared.Primitives.ErrorType.Conflict => Results.Conflict(error.Message),
        _ => Results.BadRequest(error.Message),
    };
}
