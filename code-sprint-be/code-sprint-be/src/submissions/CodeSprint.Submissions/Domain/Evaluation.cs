namespace CodeSprint.Submissions.Domain;

/// <summary>
/// The outcome of judging a <see cref="Submission"/>. Owned by the aggregate
/// (EF <c>OwnsOne</c>) and absent (<c>null</c>) until the submission reaches
/// <see cref="SubmissionStatus.Completed"/>. Produced exactly once by
/// <see cref="Submission.Complete"/> and immutable thereafter.
/// </summary>
public sealed class Evaluation
{
    private readonly List<TestCaseResult> _results = [];

    // Parameterless ctor for EF materialization.
    private Evaluation() { }

    public Evaluation(
        Verdict verdict,
        int pointsAwarded,
        int runtimeMs,
        int memoryKb,
        DateTimeOffset evaluatedAt,
        IReadOnlyList<TestCaseResult> results)
    {
        Verdict = verdict;
        PointsAwarded = pointsAwarded;
        RuntimeMs = runtimeMs;
        MemoryKb = memoryKb;
        EvaluatedAt = evaluatedAt;
        _results.AddRange(results);
    }

    public Verdict Verdict { get; private set; }
    public int PointsAwarded { get; private set; }

    /// <summary>Worst-case runtime across all test cases, in milliseconds.</summary>
    public int RuntimeMs { get; private set; }

    /// <summary>Peak memory across all test cases, in kilobytes.</summary>
    public int MemoryKb { get; private set; }

    public DateTimeOffset EvaluatedAt { get; private set; }

    public IReadOnlyList<TestCaseResult> Results => _results.AsReadOnly();
}
