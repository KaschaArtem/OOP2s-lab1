public interface IOrderQueue<T>
{
    int Count { get; }

    void Enqueue(T item);

    ValueTask<T> DequeueAsync(CancellationToken cancellationToken);
}
