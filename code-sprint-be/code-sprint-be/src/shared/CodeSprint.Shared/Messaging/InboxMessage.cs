namespace CodeSprint.Shared.Messaging;

/// <summary>
/// Idempotency record for the inbox pattern. One row per (consumer, message) that a
/// consumer has successfully processed. NOT a domain aggregate — it carries no
/// behaviour and raises no domain events. It is written in the SAME transaction as
/// the projection/side-effect it guards, so "message processed" and "effect applied"
/// commit atomically. The broker may redeliver; a duplicate hits the primary key and
/// is skipped.
/// </summary>
/// <remarks>
/// The key is composite — <c>(Consumer, MessageId)</c> — because the same integration
/// event fans out to EVERY bound consumer. Two consumers sharing one database must
/// dedup independently: keying on <see cref="MessageId"/> alone would let one
/// consumer's row block another's legitimate processing. <see cref="Consumer"/> is a
/// stable logical identity (typically the consumer's queue-name suffix, e.g.
/// <c>heatmap</c>).
/// </remarks>
public class InboxMessage
{
    /// <summary>Stable consumer identity, e.g. <c>heatmap</c>. Part of the dedup key.</summary>
    public string Consumer { get; set; } = default!;

    /// <summary>The integration event's <c>EventId</c> (its broker <c>MessageId</c>). Part of the dedup key.</summary>
    public Guid MessageId { get; set; }

    /// <summary>When this consumer finished processing the message.</summary>
    public DateTimeOffset ProcessedOn { get; set; } = DateTimeOffset.UtcNow;
}
