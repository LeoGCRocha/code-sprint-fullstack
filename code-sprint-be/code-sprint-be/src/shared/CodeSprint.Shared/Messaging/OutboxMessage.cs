namespace CodeSprint.Shared.Messaging;

/// <summary>
/// A persisted transactional-outbox row. NOT a domain aggregate — it carries no
/// behaviour, raises no domain events, and is never reached through an aggregate
/// root. It is a plain persistence record written in the SAME transaction as the
/// state change that produced it, so the integration event and the business data
/// commit atomically. A separate publisher (not in this scaffold) later reads
/// unprocessed rows and dispatches them to the broker.
/// </summary>
/// <remarks>
/// Public, settable properties and an implicit parameterless constructor keep the
/// shape trivially materializable by EF Core. The contract type is stored by name
/// (e.g. <c>nameof(SubmissionEvaluatedV1)</c>) alongside its JSON payload so the
/// publisher can resolve and deserialize it without coupling to the writer.
/// </remarks>
public class OutboxMessage
{
    /// <summary>Primary key. Time-ordered (UUIDv7) so rows insert and scan in occurrence order.</summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>The integration-event contract type name, e.g. <c>nameof(SubmissionEvaluatedV1)</c>.</summary>
    public string Type { get; set; } = default!;

    /// <summary>The serialized JSON payload of the integration event.</summary>
    public string Content { get; set; } = default!;

    /// <summary>When the originating event occurred (mirrors the contract's OccurredOn).</summary>
    public DateTimeOffset OccurredOn { get; set; }

    /// <summary><c>null</c> until the publisher has successfully dispatched the message.</summary>
    public DateTimeOffset? ProcessedOn { get; set; }

    /// <summary>The last publish error, if any; <c>null</c> while no failure has occurred.</summary>
    public string? Error { get; set; }

    /// <summary>Number of failed publish attempts so far. Drives retry/back-off policy.</summary>
    public int RetryCount { get; set; }
}
