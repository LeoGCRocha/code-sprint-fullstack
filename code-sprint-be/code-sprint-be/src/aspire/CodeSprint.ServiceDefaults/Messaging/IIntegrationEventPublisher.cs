using CodeSprint.Shared.Contracts;

namespace CodeSprint.ServiceDefaults.Messaging;

/// <summary>
/// Low-level transport for pushing a single integration event onto the broker.
/// This is pure transport: it does NOT read an outbox, batch, retry, or guarantee
/// delivery beyond what a single AMQP publish provides. The outbox-drain loop that
/// calls this (one event at a time) is written separately.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Serializes <paramref name="evt"/> and publishes it to the
    /// <see cref="MessagingTopology.ExchangeName"/> topic exchange under
    /// <paramref name="routingKey"/> with persistent delivery.
    /// </summary>
    /// <param name="evt">The integration event to publish.</param>
    /// <param name="routingKey">
    /// The topic routing key (e.g. <see cref="MessagingTopology.SubmissionEvaluatedRoutingKey"/>).
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync(IIntegrationEvent evt, string routingKey, CancellationToken ct = default);
}
