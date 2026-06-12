using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSprint.Shared.Messaging;

/// <summary>
/// EF Core mapping for <see cref="InboxMessage"/>, shared across bounded contexts.
///
/// Maps to the <c>inbox_messages</c> table with snake_case columns. No schema is
/// hardcoded — the table inherits the owning <see cref="DbContext"/>'s default
/// schema, so each consuming bounded context keeps its inbox inside its own schema.
/// The composite primary key <c>(consumer, message_id)</c> IS the deduplication
/// guarantee: a redelivered message produces a duplicate insert that violates the
/// key, which the consumer treats as "already processed".
/// </summary>
public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");

        // Composite key: the SAME event is delivered to every bound consumer; each
        // dedups on its own (consumer, message) pair.
        builder.HasKey(m => new { m.Consumer, m.MessageId });

        builder.Property(m => m.Consumer)
            .HasColumnName("consumer")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.MessageId)
            .HasColumnName("message_id")
            .IsRequired();

        builder.Property(m => m.ProcessedOn)
            .HasColumnName("processed_on")
            .IsRequired();
    }
}
