using System.Text.Json;
using CodeSprint.Shared.Contracts;
using RabbitMQ.Client;

namespace CodeSprint.ServiceDefaults.Messaging;

/// <summary>
/// <see cref="IIntegrationEventPublisher"/> backed by the official
/// <c>RabbitMQ.Client</c> (v7 async API). Resolves the shared
/// <see cref="IConnection"/> from DI (registered by the Aspire
/// <c>AddRabbitMQClient</c> integration) and opens a fresh channel per publish —
/// <see cref="IChannel"/> instances are not thread-safe, so sharing one across
/// concurrent publishers is unsafe; the connection, however, is safe to share.
/// </summary>
/// <remarks>
/// Pure transport: declares the exchange idempotently and pushes exactly one event.
/// It does not own retry, outbox draining, or consumption.
/// </remarks>
public sealed class RabbitMqIntegrationEventPublisher : IIntegrationEventPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnection _connection;

    /// <summary>
    /// Creates the publisher over the shared broker connection supplied by DI.
    /// </summary>
    public RabbitMqIntegrationEventPublisher(IConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc />
    public async Task PublishAsync(IIntegrationEvent evt, string routingKey, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(evt);
        ArgumentException.ThrowIfNullOrWhiteSpace(routingKey);

        // A channel is cheap and not thread-safe, so create one per publish and dispose it.
        await using var channel = await _connection.CreateChannelAsync(cancellationToken: ct)
            .ConfigureAwait(false);

        // Idempotent declare: ensures the durable topic exchange exists before we publish.
        await channel.ExchangeDeclareAsync(
                exchange: MessagingTopology.ExchangeName,
                type: MessagingTopology.ExchangeType,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct)
            .ConfigureAwait(false);

        // Serialize on the concrete runtime type so every contract property is emitted.
        var body = JsonSerializer.SerializeToUtf8Bytes(evt, evt.GetType(), SerializerOptions);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            // Persisted to disk so the message survives a broker restart (paired with the durable exchange/queue).
            Persistent = true,
            MessageId = evt.EventId.ToString(),
            // The runtime type name (e.g. "SubmissionEvaluatedV1") lets consumers dispatch without sniffing the body.
            Type = evt.GetType().Name,
        };

        await channel.BasicPublishAsync(
                exchange: MessagingTopology.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: ct)
            .ConfigureAwait(false);
    }
}
