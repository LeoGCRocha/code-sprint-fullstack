namespace CodeSprint.Shared.Primitives;

/// <summary>
/// Convenience base for domain events. Stamps <see cref="EventId"/> and
/// <see cref="OccurredOn"/> on construction. Derive with a positional record:
/// <c>public sealed record UserRegistered(UserId UserId) : DomainEvent;</c>
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
