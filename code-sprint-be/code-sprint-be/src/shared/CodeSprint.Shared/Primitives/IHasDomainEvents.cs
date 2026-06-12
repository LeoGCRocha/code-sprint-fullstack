namespace CodeSprint.Shared.Primitives;

/// <summary>
/// Non-generic view of an aggregate's recorded domain events, so infrastructure
/// (e.g. an outbox interceptor) can collect them without knowing the id type.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
