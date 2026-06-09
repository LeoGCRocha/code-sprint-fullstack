namespace CodeSprint.Submissions.Domain;

/// <summary>
/// The result of running the submission against a single test case. Owned by the
/// aggregate (persisted under the submission). For hidden tests, only pass/fail and
/// timing are kept — <see cref="ActualOutput"/> is never stored, so a caller can
/// never reconstruct the hidden case's I/O.
/// </summary>
public sealed class TestCaseResult
{
    // Parameterless ctor for EF materialization.
    public TestCaseResult() { }

    public TestCaseResult(
        int ordinal,
        TestCaseStatus status,
        int runtimeMs,
        int memoryKb,
        bool isHidden,
        string? actualOutput)
    {
        Ordinal = ordinal;
        Status = status;
        RuntimeMs = runtimeMs;
        MemoryKb = memoryKb;
        IsHidden = isHidden;
        // Redact I/O for hidden tests at the boundary, not just at the API edge.
        ActualOutput = isHidden ? null : actualOutput;
    }

    public int Ordinal { get; init; }
    public TestCaseStatus Status { get; init; }
    public int RuntimeMs { get; init; }
    public int MemoryKb { get; init; }
    public bool IsHidden { get; init; }
    public string? ActualOutput { get; init; }
}
