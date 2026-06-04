namespace CodeSprint.Shared.Contracts;

/// <summary>
/// Published by the Submission context when a submission has been judged. The
/// single integration event feeding both read models: PlayerProgression (Users
/// BC) and solvedCount (Problems BC). Consumers must be idempotent — key on
/// <see cref="SubmissionId"/>, as the broker may redeliver.
/// </summary>
/// <remarks>Versioned by type name (…V1). A breaking change ships as a new …V2 type.</remarks>
public sealed record SubmissionEvaluatedV1 : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;

    public required Guid SubmissionId { get; init; }

    public required Guid UserId { get; init; }

    public required Guid ProblemId { get; init; }

    /// <summary>One of: <c>accepted</c>, <c>wrong</c>, <c>error</c>, <c>timeout</c>.</summary>
    public required string Verdict { get; init; }

    public required int PointsAwarded { get; init; }

    public required string Language { get; init; }

    public required DateTimeOffset EvaluatedAt { get; init; }
}
