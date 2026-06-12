using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Shared.Messaging;

/// <summary>
/// Opt-in hook for adding the inbox (consumer-side deduplication) to any bounded
/// context's model.
///
/// Because <see cref="InboxMessageConfiguration"/> lives in this Shared assembly,
/// a context's <c>ApplyConfigurationsFromAssembly(thisAssembly)</c> will NOT pick
/// it up — call <see cref="AddInbox"/> explicitly in <c>OnModelCreating</c> to map
/// the <c>inbox_messages</c> table into the context's default schema. Only consuming
/// contexts need it (the inbox lives at the READ end, the outbox at the WRITE end).
/// </summary>
public static class InboxModelExtensions
{
    public static ModelBuilder AddInbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        return modelBuilder;
    }
}
