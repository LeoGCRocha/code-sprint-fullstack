namespace CodeSprint.Submissions.API.Endpoints.Submissions;

/// <summary>Body for <c>POST /submissions</c>.</summary>
public record CreateSubmissionRequest(string ProblemId, string Language, string SourceCode);

/// <summary>Acknowledgement returned (202) when a submission is accepted for judging.</summary>
public record CreateSubmissionResponse(string Id, string Status);

/// <summary>Generic paged envelope, mirroring the Problems BC shape.</summary>
public record PagedResponse<T>(List<T> Items, int Total, int Page, int PageSize);

/// <summary>Per-test result in a submission's evaluation. Hidden tests never expose I/O.</summary>
public record TestCaseResultResponse(
    int Ordinal,
    string Status,
    int RuntimeMs,
    int MemoryKb,
    bool IsHidden,
    string? ActualOutput);

/// <summary>The judged outcome of a submission. Present only when Completed.</summary>
public record EvaluationResponse(
    string Verdict,
    int PointsAwarded,
    int RuntimeMs,
    int MemoryKb,
    DateTimeOffset EvaluatedAt,
    List<TestCaseResultResponse> Results);

/// <summary>Full submission view for <c>GET /submissions/{id}</c>.</summary>
public record SubmissionResponse(
    string Id,
    string ProblemId,
    string Language,
    string Status,
    DateTimeOffset SubmittedAt,
    EvaluationResponse? Evaluation);

/// <summary>Summary row for the user's submission list.</summary>
public record SubmissionSummaryResponse(
    string Id,
    string ProblemId,
    string Language,
    string Status,
    DateTimeOffset SubmittedAt,
    string? Verdict,
    int? PointsAwarded);
