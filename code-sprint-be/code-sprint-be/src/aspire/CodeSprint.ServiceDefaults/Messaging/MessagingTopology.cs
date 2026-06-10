namespace CodeSprint.ServiceDefaults.Messaging;

/// <summary>
/// Single source of truth for the CodeSprint RabbitMQ topology. Both publishers
/// (in any bounded context) and the human-authored consumers must declare against
/// these names so producer and consumer agree on the wire contract.
/// </summary>
/// <remarks>
/// The exchange is a durable <c>topic</c> exchange so consumers can bind with
/// routing-key patterns (e.g. <c>submission.*</c>) and survive a broker restart.
/// </remarks>
public static class MessagingTopology
{
    /// <summary>
    /// The durable topic exchange every integration event is published to.
    /// </summary>
    public const string ExchangeName = "codesprint.events";

    /// <summary>
    /// The exchange type passed to <c>ExchangeDeclareAsync</c>. Topic exchanges
    /// route on dotted routing-key patterns.
    /// </summary>
    public const string ExchangeType = "topic";

    /// <summary>
    /// Routing key for <c>SubmissionEvaluatedV1</c>. Consumers interested in all
    /// submission events can bind with the pattern <c>submission.*</c>.
    /// </summary>
    public const string SubmissionEvaluatedRoutingKey = "submission.evaluated";

    /// <summary>
    /// Builds the canonical durable queue name for a consumer. Naming queues per
    /// consumer (rather than sharing one) gives each read-model its own backlog and
    /// independent acking. Format: <c>{exchange}.{consumer}</c>.
    /// </summary>
    /// <param name="consumerName">
    /// A stable identifier for the consumer, e.g. <c>player-progression</c> or
    /// <c>problem-solved-count</c>. Must be unique per logical read model.
    /// </param>
    /// <returns>A durable queue name, e.g. <c>codesprint.events.player-progression</c>.</returns>
    public static string QueueNameFor(string consumerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerName);
        return $"{ExchangeName}.{consumerName}";
    }
}
