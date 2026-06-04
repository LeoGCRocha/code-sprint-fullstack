namespace CodeSprint.Shared.Contracts;

/// <summary>
/// Marker + envelope metadata for messages published across bounded contexts
/// (RabbitMQ). Unlike domain events, integration-event contracts are PUBLIC,
/// VERSIONED, and use primitive types only — never strongly-typed IDs or domain
/// objects — so they stay stable on the wire as each side evolves independently.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOn { get; }
}
