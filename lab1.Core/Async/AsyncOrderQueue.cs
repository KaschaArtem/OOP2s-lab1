using System.Threading.Channels;

namespace lab1.Core.Async;

public sealed class AsyncOrderQueue<T> : IOrderQueue<T>
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    private readonly object _gate = new();
    private readonly LinkedList<T> _buffered = new();

    public int Count
    {
        get { lock (_gate) return _buffered.Count; }
    }

    public IReadOnlyList<T> Snapshot()
    {
        lock (_gate)
        {
            return _buffered.ToArray();
        }
    }

    public void Enqueue(T item)
    {
        lock (_gate)
        {
            _buffered.AddLast(item);
        }

        _channel.Writer.TryWrite(item);
    }

    public async ValueTask<T> DequeueAsync(CancellationToken cancellationToken)
    {
        try
        {
            var item = await _channel.Reader.ReadAsync(cancellationToken);
            lock (_gate)
            {
                _buffered.Remove(item);
            }

            return item;
        }
        catch (ChannelClosedException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
    }
}
