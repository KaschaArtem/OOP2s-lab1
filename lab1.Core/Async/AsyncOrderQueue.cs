using System.Threading.Channels;

namespace lab1.Core.Async;

public sealed class AsyncOrderQueue<T> : IOrderQueue<T>
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>(
        new UnboundedChannelOptions { SingleReader = false, SingleWriter = false });

    public int Count => _channel.Reader.Count;

    public void Enqueue(T item) => _channel.Writer.TryWrite(item);

    public async ValueTask<T> DequeueAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }
        catch (ChannelClosedException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
    }
}
