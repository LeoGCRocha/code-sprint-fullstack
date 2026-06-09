using CodeSprint.Shared.Ids;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Submissions.Domain.Events;

/// <summary>
/// Raised when a submission is accepted for judging. The application layer reacts
/// by enqueuing the judge job.
/// </summary>
public sealed record SubmissionCreated(SubmissionId SubmissionId) : DomainEvent;
