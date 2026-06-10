using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSprint.Shared.Messaging;

/// <summary>
/// EF Core mapping for <see cref="OutboxMessage"/>, shared across bounded contexts.
///
/// Maps to the <c>outbox_messages</c> table with snake_case columns. No schema is
/// hardcoded — the table inherits the owning <see cref="DbContext"/>'s default
/// schema, so each bounded context keeps its outbox inside its own schema. The
/// index on <see cref="OutboxMessage.ProcessedOn"/> supports the publisher's
/// hot-path query for unprocessed rows.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.Type)
            .HasColumnName("type")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(m => m.OccurredOn)
            .HasColumnName("occurred_on")
            .IsRequired();

        builder.Property(m => m.ProcessedOn)
            .HasColumnName("processed_on");

        builder.Property(m => m.Error)
            .HasColumnName("error");

        builder.Property(m => m.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        // Supports the publisher's "fetch unprocessed messages" query
        // (WHERE processed_on IS NULL ORDER BY occurred_on).
        builder.HasIndex(m => m.ProcessedOn);
    }
}
