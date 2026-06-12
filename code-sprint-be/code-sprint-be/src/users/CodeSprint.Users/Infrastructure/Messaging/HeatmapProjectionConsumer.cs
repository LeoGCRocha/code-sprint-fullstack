using System.Text.Json;
using CodeSprint.ServiceDefaults.Messaging;
using CodeSprint.Shared.Contracts;
using CodeSprint.Shared.Ids;
using CodeSprint.Shared.Messaging;
using CodeSprint.Users.Infrastructure;
using CodeSprint.Users.ReadModels;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CodeSprint.Users.Infrastructure.Messaging;

/// <summary>
/// READ-side consumer that feeds the Users heatmap read model. It owns a durable
/// queue (<c>codesprint.events.heatmap</c>) bound to the shared durable topic
/// exchange on the <c>submission.evaluated</c> routing key, consuming
/// <see cref="SubmissionEvaluatedV1"/> integration events with manual
/// acknowledgement (autoAck:false).
/// </summary>
/// <remarks>
/// <para>
/// This class is pure transport plumbing: it declares the topology, deserializes
/// each delivery, hands the event to the projection seam, and acks/nacks based on
/// the outcome. The single broker <see cref="IConnection"/> is shared from DI; the
/// channel is opened once and lives for the lifetime of the background service
/// (a channel is not thread-safe, but RabbitMQ dispatches deliveries for one
/// consumer sequentially, so a single owned channel is correct here).
/// </para>
/// <para>
/// The projection itself (<see cref="ProjectAsync"/>) is a deliberate SEAM: dedup
/// (inbox/ProcessedMessage keyed on <c>EventId</c>) and the HEATMAP_DAY projection
/// are intentionally left unimplemented for hand-authoring. Until then the pipeline
/// runs end-to-end as a no-op.
/// </para>
/// </remarks>
public sealed class HeatmapProjectionConsumer : BackgroundService
{
    /// <summary>Stable consumer identity used to derive the durable queue name.</summary>
    private const string ConsumerName = "heatmap";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HeatmapProjectionConsumer> _logger;

    /// <summary>
    /// The long-lived channel for this consumer. Opened in <see cref="ExecuteAsync"/>
    /// and disposed on shutdown. Not disposed per message (unlike the publisher).
    /// </summary>
    private IChannel? _channel;

    /// <summary>
    /// Creates the consumer over the shared broker connection supplied by DI.
    /// </summary>
    public HeatmapProjectionConsumer(
        IConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<HeatmapProjectionConsumer> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queue = MessagingTopology.QueueNameFor(ConsumerName);

        // Open the channel once; it lives for the service lifetime.
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken)
            .ConfigureAwait(false);

        // Idempotent declare: ensures the durable topic exchange exists (matches the publisher).
        await _channel.ExchangeDeclareAsync(
                exchange: MessagingTopology.ExchangeName,
                type: MessagingTopology.ExchangeType,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken)
            .ConfigureAwait(false);

        // Durable, non-exclusive queue so this read model keeps its own backlog across restarts.
        await _channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken)
            .ConfigureAwait(false);

        // Bind the queue to the topic exchange for the judged-submission routing key.
        await _channel.QueueBindAsync(
                queue: queue,
                exchange: MessagingTopology.ExchangeName,
                routingKey: MessagingTopology.SubmissionEvaluatedRoutingKey,
                arguments: null,
                cancellationToken: stoppingToken)
            .ConfigureAwait(false);

        // Cap unacked messages in flight so a slow projection doesn't pull the whole backlog.
        await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 10,
                global: false,
                cancellationToken: stoppingToken)
            .ConfigureAwait(false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnReceivedAsync;

        // Manual ack (autoAck:false): we ack only after the projection seam succeeds.
        await _channel.BasicConsumeAsync(
                queue: queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Heatmap projection consumer listening on queue {Queue} bound to {Exchange}/{RoutingKey}.",
            queue,
            MessagingTopology.ExchangeName,
            MessagingTopology.SubmissionEvaluatedRoutingKey);

        // Keep the background service alive until cancellation; deliveries arrive on the consumer.
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown signal — fall through to let the service stop.
        }
    }

    /// <summary>
    /// Handles a single delivery: deserialize, run the projection seam, then ack.
    /// </summary>
    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        // The channel is created before consuming starts, so it is non-null here.
        var channel = _channel!;

        try
        {
            var evt = JsonSerializer.Deserialize<SubmissionEvaluatedV1>(ea.Body.Span, SerializerOptions);
            if (evt is null)
            {
                _logger.LogWarning(
                    "Discarding heatmap message {DeliveryTag}: body deserialized to null.",
                    ea.DeliveryTag);

                // Poison/undeserializable payload: drop it, do not requeue.
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false)
                    .ConfigureAwait(false);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

            // SEAM: hand the event to the (currently no-op) projection.
            await ProjectAsync(evt, db, CancellationToken.None).ConfigureAwait(false);

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to project heatmap message {DeliveryTag}; requeuing for redelivery.",
                ea.DeliveryTag);

            // Transient failure: requeue so it is redelivered.
            // NOTE: a permanently-failing message will hot-loop here. The human may
            // later add a dead-letter exchange / poison-message policy (e.g. nack with
            // requeue:false after N attempts, routing to a DLQ).
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Projects a judged submission into the heatmap read model.
    /// </summary>
    private static async Task ProjectAsync(SubmissionEvaluatedV1 evt, UsersDbContext db, CancellationToken ct)
    {
        // 1. Dedup gate — already processed by THIS consumer?
        var already = await db.InboxMessages
            .AnyAsync(m => m.Consumer == ConsumerName && m.MessageId == evt.EventId, ct);
        if (already)
            return;

        // 2. Record + project atomically. The inbox row (dedup) and the count
        // upsert commit in ONE transaction, so a crash can't count without
        // recording the message (or vice-versa).
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        db.InboxMessages.Add(new InboxMessage
        {
            Consumer = ConsumerName,
            MessageId = evt.EventId,
        });
        await db.SaveChangesAsync(ct);

        // 3. Atomic DB-side upsert. The increment happens IN the database, not via
        // read-modify-write in memory — so the first event counts as 1, and
        // concurrent deliveries (e.g. a redelivery burst dispatched in parallel)
        // increment without clobbering each other (no lost updates).
        var userId = new UserId(evt.UserId);
        var day = DateOnly.FromDateTime(evt.EvaluatedAt.UtcDateTime);

        await db.Database.ExecuteSqlAsync(
            $@"INSERT INTO users.heatmap_days (user_id, day, count)
               VALUES ({userId.Value}, {day}, 1)
               ON CONFLICT (user_id, day)
               DO UPDATE SET count = heatmap_days.count + 1", ct);

        await tx.CommitAsync(ct);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}
