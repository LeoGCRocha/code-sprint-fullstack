using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Shared.Messaging;

/// <summary>
/// Opt-in hook for adding the transactional outbox to any bounded context's model.
///
/// Because <see cref="OutboxMessageConfiguration"/> lives in this Shared assembly,
/// a context's <c>ApplyConfigurationsFromAssembly(thisAssembly)</c> will NOT pick
/// it up — call <see cref="AddOutbox"/> explicitly in <c>OnModelCreating</c> to map
/// the <c>outbox_messages</c> table into the context's default schema.
/// </summary>
public static class OutboxModelExtensions
{
    public static ModelBuilder AddOutbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        return modelBuilder;
    }
}
