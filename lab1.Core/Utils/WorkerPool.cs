namespace lab1.Core.Utils;

/// <summary>
/// Keeps an ordered set of workers and hands out the first one that is currently
/// free. If the first worker is busy, the next one is taken, and so on. When every
/// worker is busy the caller waits until one of them is released.
/// </summary>
public sealed class WorkerPool<T> where T : class
{
    private readonly IReadOnlyList<T> _workers;
    private readonly bool[] _idle;
    private readonly object _gate = new();
    private TaskCompletionSource _available = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public WorkerPool(IReadOnlyList<T> workers)
    {
        _workers = workers;
        _idle = new bool[workers.Count];
        Array.Fill(_idle, true);
    }

    public async Task<T> AcquireAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            Task wait;

            lock (_gate)
            {
                for (var i = 0; i < _workers.Count; i++)
                {
                    if (_idle[i])
                    {
                        _idle[i] = false;
                        return _workers[i];
                    }
                }

                wait = _available.Task;
            }

            await wait.WaitAsync(cancellationToken);
        }
    }

    public void Release(T worker)
    {
        lock (_gate)
        {
            for (var i = 0; i < _workers.Count; i++)
            {
                if (ReferenceEquals(_workers[i], worker))
                {
                    _idle[i] = true;
                    break;
                }
            }

            var previous = _available;
            _available = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            previous.TrySetResult();
        }
    }
}
