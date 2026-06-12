namespace CodeSprint.Shared.Primitives;

/// <summary>
/// The consistency boundary and transactional unit. Only aggregate roots are
/// loaded and saved by repositories; everything inside is reached through the
/// root. Records domain events for the infrastructure layer to dispatch after
/// the unit of work commits.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id) { }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
