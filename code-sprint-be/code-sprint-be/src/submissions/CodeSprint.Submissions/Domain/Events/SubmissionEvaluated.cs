using CodeSprint.Shared.Ids;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Submissions.Domain.Events;

/// <summary>
/// Raised when a submission has been judged. Carries everything the outbox needs to
/// build the public <c>SubmissionEvaluatedV1</c> integration event without reloading
/// the aggregate. In-context only — see <c>CodeSprint.Shared.Contracts</c> for the
/// cross-context contract.
/// </summary>
public sealed record SubmissionEvaluated(
    SubmissionId SubmissionId,
    UserId UserId,
    ProblemId ProblemId,
    Verdict Verdict,
    int PointsAwarded,
    Language Language,
    DateTimeOffset EvaluatedAt) : DomainEvent;
