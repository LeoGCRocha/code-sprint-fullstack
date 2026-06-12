using System.Text.Json;
using CodeSprint.Shared.Messaging;
using CodeSprint.Shared.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CodeSprint.Submissions.Infrastructure.Outbox;

/// <summary>
/// Collects domain events raised by aggregates in the current unit of work and
/// writes them as <see cref="OutboxMessage"/> rows IN THE SAME SaveChanges (hence
/// the same transaction) as the state change that produced them — the transactional
/// outbox. A separate publisher later drains the table to the broker.
/// </summary>
public sealed class ConvertDomainEventsToOutboxInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            WriteOutboxMessages(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // Cover synchronous saves too, so the outbox stays correct regardless of call site.
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            WriteOutboxMessages(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private static void WriteOutboxMessages(DbContext context)
    {
        var aggregates = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var integrationEvent = IntegrationEventFactory.TryCreate(domainEvent);
                if (integrationEvent is null)
                    continue; // e.g. SubmissionCreated never leaves the context

                context.Set<OutboxMessage>().Add(new OutboxMessage
                {
                    Type = integrationEvent.GetType().Name,
                    Content = JsonSerializer.Serialize(
                        integrationEvent, integrationEvent.GetType(), SerializerOptions),
                    OccurredOn = integrationEvent.OccurredOn,
                });
            }

            aggregate.ClearDomainEvents();
        }
    }
}
