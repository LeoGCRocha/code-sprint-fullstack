namespace CodeSprint.Submissions.Domain;

/// <summary>
/// The raw result the judge brings back for a submission, before the aggregate
/// applies scoring rules. Carries the verdict and per-test detail; the aggregate
/// decides <c>PointsAwarded</c> in <see cref="Submission.Complete"/>.
/// </summary>
public sealed record JudgeOutcome(
    Verdict Verdict,
    int RuntimeMs,
    int MemoryKb,
    IReadOnlyList<TestCaseResult> Results);
