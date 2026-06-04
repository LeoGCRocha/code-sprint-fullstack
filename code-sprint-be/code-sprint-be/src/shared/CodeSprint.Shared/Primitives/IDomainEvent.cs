namespace CodeSprint.Shared.Primitives;

/// <summary>
/// A fact that happened inside a single bounded context. Raised by an
/// <see cref="AggregateRoot{TId}"/> and dispatched in-process after the
/// owning transaction commits. NOT for cross-context messaging — see
/// <c>CodeSprint.Shared.Contracts</c> for integration events.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOn { get; }
}
