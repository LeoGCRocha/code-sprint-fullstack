using System.Text.Json;
using CodeSprint.ServiceDefaults.Messaging;
using CodeSprint.Shared.Contracts;
using CodeSprint.Shared.Messaging;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Submissions.Infrastructure.Outbox;

/// <summary>
/// Drains the transactional outbox: polls <see cref="OutboxMessage"/> rows the
/// interceptor wrote in the same transaction as the state change, publishes each
/// to the broker, then marks it processed. Delivery is AT-LEAST-ONCE — we publish
/// THEN mark, so a crash in between redelivers on the next poll. Dedup is the
/// consumer's job (key on the contract id), never ours.
/// </summary>
public sealed class OutboxPublisher(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisher> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private const int BatchSize = 20;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break; // clean shutdown
            }
            catch (Exception ex)
            {
                // One bad poll must not kill the loop — log and keep draining.
                logger.LogError(ex, "Outbox poll failed; retrying after {Interval}.", PollInterval);
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break; // clean shutdown
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SubmissionsDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

        var batch = await db.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (batch.Count == 0)
            return;

        foreach (var row in batch)
        {
            try
            {
                var (clrType, routingKey) = Resolve(row.Type);
                if (clrType is null)
                {
                    // Unknown contract — don't hot-loop forever; record and skip.
                    logger.LogWarning("Outbox row {Id} has unknown type {Type}; skipping.", row.Id, row.Type);
                    row.Error = "unknown type";
                    row.RetryCount++;
                    continue;
                }

                var evt = (IIntegrationEvent)JsonSerializer.Deserialize(row.Content, clrType, SerializerOptions)!;
                await publisher.PublishAsync(evt, routingKey, ct);
                row.ProcessedOn = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                // Per-row failure: record and move on, leaving it unprocessed for retry.
                logger.LogWarning(ex, "Failed to publish outbox row {Id} ({Type}).", row.Id, row.Type);
                row.Error = ex.Message;
                row.RetryCount++;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Maps the stored contract type NAME to its CLR type + routing key. A null
    /// CLR type means the name is unknown to this publisher.
    /// </summary>
    private static (Type? ClrType, string RoutingKey) Resolve(string type) => type switch
    {
        nameof(SubmissionEvaluatedV1) => (typeof(SubmissionEvaluatedV1), MessagingTopology.SubmissionEvaluatedRoutingKey),
        _ => (null, string.Empty),
    };
}
