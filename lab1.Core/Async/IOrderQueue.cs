public interface IReadOnlyOrderQueue<T>
{
    int Count { get; }

    IReadOnlyList<T> Snapshot();
}

public interface IOrderQueue<T> : IReadOnlyOrderQueue<T>
{
    void Enqueue(T item);

    ValueTask<T> DequeueAsync(CancellationToken cancellationToken);
}
