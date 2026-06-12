using CodeSprint.Shared.Contracts;
using CodeSprint.Shared.Primitives;
using CodeSprint.Submissions.Domain;
using CodeSprint.Submissions.Domain.Events;

namespace CodeSprint.Submissions.Infrastructure.Outbox;

/// <summary>
/// Maps in-context domain events to their PUBLIC integration-event contracts.
/// Returns null for domain events that do not cross the bounded-context boundary.
/// </summary>
internal static class IntegrationEventFactory
{
    public static IIntegrationEvent? TryCreate(IDomainEvent domainEvent) => domainEvent switch
    {
        SubmissionEvaluated e => new SubmissionEvaluatedV1
        {
            SubmissionId = e.SubmissionId.Value,
            UserId = e.UserId.Value,
            ProblemId = e.ProblemId.Value,
            Verdict = MapVerdict(e.Verdict),
            PointsAwarded = e.PointsAwarded,
            Language = e.Language.ToString(),
            EvaluatedAt = e.EvaluatedAt,
            // EventId + OccurredOn default in the contract's initializer.
        },
        _ => null,
    };

    // Lossy: the domain Verdict is richer than the wire contract's 4 values.
    private static string MapVerdict(Verdict verdict) => verdict switch
    {
        Verdict.Accepted => "accepted",
        Verdict.WrongAnswer => "wrong",
        Verdict.TimeLimitExceeded => "timeout",
        Verdict.RuntimeError or Verdict.MemoryLimitExceeded or Verdict.CompileError => "error",
        _ => "error",
    };
}
