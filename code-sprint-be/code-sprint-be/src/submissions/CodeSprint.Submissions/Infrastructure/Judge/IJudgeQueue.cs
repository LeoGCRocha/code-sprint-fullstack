using System.Threading.Channels;
using CodeSprint.Shared.Ids;

namespace CodeSprint.Submissions.Infrastructure.Judge;

/// <summary>
/// In-process work queue handing freshly created submission ids to the judge
/// worker. Fase 1 only: backed by an unbounded <see cref="Channel{T}"/>, so work
/// is lost on process restart. Fase 2 replaces this with a durable transport
/// (RabbitMQ + outbox) — keep producers depending on this abstraction so the swap
/// is a registration change.
/// </summary>
public interface IJudgeQueue
{
    ValueTask EnqueueAsync(SubmissionId id, CancellationToken ct = default);

    ChannelReader<SubmissionId> Reader { get; }
}

/// <summary>Singleton in-memory implementation of <see cref="IJudgeQueue"/>.</summary>
public sealed class InMemoryJudgeQueue : IJudgeQueue
{
    private readonly Channel<SubmissionId> _channel =
        Channel.CreateUnbounded<SubmissionId>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });

    public ValueTask EnqueueAsync(SubmissionId id, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(id, ct);

    public ChannelReader<SubmissionId> Reader => _channel.Reader;
}
