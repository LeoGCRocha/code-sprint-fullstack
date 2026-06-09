using CodeSprint.Shared.Ids;
using CodeSprint.Shared.Primitives;
using CodeSprint.Submissions.Domain.Events;

namespace CodeSprint.Submissions.Domain;

/// <summary>
/// A single attempt by a user at a problem. Aggregate root and consistency boundary
/// for its <see cref="Evaluation"/> and per-test results. References the user and
/// problem by value only (cross-context) — never as navigations.
///
/// Lifecycle: <c>Pending → Running → Completed</c>, with <c>Pending|Running → Failed</c>
/// for an infrastructure failure of the judge (distinct from a
/// <see cref="Verdict.RuntimeError"/>, which is a legitimate result of user code).
/// </summary>
public sealed class Submission : AggregateRoot<SubmissionId>
{
    // Private ctor shared by Create and EF constructor binding.
    private Submission(
        SubmissionId id,
        UserId userId,
        ProblemId problemId,
        Language language,
        SourceCode sourceCode,
        DateTimeOffset submittedAt) : base(id)
    {
        UserId = userId;
        ProblemId = problemId;
        Language = language;
        SourceCode = sourceCode;
        SubmittedAt = submittedAt;
        Status = SubmissionStatus.Pending;
    }

    public UserId UserId { get; private set; }
    public ProblemId ProblemId { get; private set; }
    public Language Language { get; private set; }
    public SourceCode SourceCode { get; private set; }
    public SubmissionStatus Status { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }

    /// <summary>The judged outcome. <c>null</c> until <see cref="Status"/> is Completed.</summary>
    public Evaluation? Evaluation { get; private set; }

    /// <summary>Reason the judge infrastructure failed. Set only when Failed.</summary>
    public string? FailureReason { get; private set; }

    /// <summary>Creates a submission in the <see cref="SubmissionStatus.Pending"/> state.</summary>
    public static Result<Submission> Create(
        UserId userId,
        ProblemId problemId,
        Language language,
        SourceCode sourceCode)
    {
        var submission = new Submission(
            SubmissionId.New(), userId, problemId, language, sourceCode, DateTimeOffset.UtcNow);

        submission.Raise(new SubmissionCreated(submission.Id));
        return submission;
    }

    /// <summary>Pending → Running. Called when the judge picks the job up.</summary>
    public Result Start()
    {
        if (Status != SubmissionStatus.Pending)
            return Result.Failure(Error.Validation("submission.start.invalidState",
                $"Cannot start a submission in state '{Status}'"));

        Status = SubmissionStatus.Running;
        return Result.Success();
    }

    /// <summary>
    /// Running → Completed. Applies the scoring rule: full problem points are awarded
    /// only on the user's FIRST accepted solve of this problem; zero otherwise. The
    /// "first solve" determination is supplied by the application layer (it queries the
    /// submission history), keeping the rule's authority inside this context.
    /// </summary>
    public Result Complete(JudgeOutcome outcome, int problemPoints, bool isFirstAcceptedSolve)
    {
        if (Status != SubmissionStatus.Running)
            return Result.Failure(Error.Validation("submission.complete.invalidState",
                $"Cannot complete a submission in state '{Status}'"));

        var pointsAwarded = outcome.Verdict == Verdict.Accepted && isFirstAcceptedSolve
            ? problemPoints
            : 0;

        Evaluation = new Evaluation(
            outcome.Verdict,
            pointsAwarded,
            outcome.RuntimeMs,
            outcome.MemoryKb,
            DateTimeOffset.UtcNow,
            outcome.Results);

        Status = SubmissionStatus.Completed;

        Raise(new SubmissionEvaluated(
            Id, UserId, ProblemId, outcome.Verdict, pointsAwarded, Language, Evaluation.EvaluatedAt));

        return Result.Success();
    }

    /// <summary>
    /// Pending|Running → Failed. For an infrastructure failure of the judge itself —
    /// NOT a user-code error, which is a <see cref="Verdict"/>. Retry is a later phase.
    /// </summary>
    public Result Fail(string reason)
    {
        if (Status is SubmissionStatus.Completed or SubmissionStatus.Failed)
            return Result.Failure(Error.Validation("submission.fail.invalidState",
                $"Cannot fail a submission in state '{Status}'"));

        Status = SubmissionStatus.Failed;
        FailureReason = reason;
        return Result.Success();
    }
}
