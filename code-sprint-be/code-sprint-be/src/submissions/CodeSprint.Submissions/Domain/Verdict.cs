namespace CodeSprint.Submissions.Domain;

/// <summary>
/// The judged result of a submission's code. Richer than the wire contract
/// <c>SubmissionEvaluatedV1.Verdict</c> (which has only accepted/wrong/error/timeout);
/// the lossy mapping to that string happens when the integration event is published.
/// </summary>
public enum Verdict
{
    Accepted,
    WrongAnswer,
    RuntimeError,
    TimeLimitExceeded,
    MemoryLimitExceeded,
    CompileError,
}
