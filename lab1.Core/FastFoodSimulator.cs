using lab1.Core.Async;
using lab1.Core.Models;
using lab1.Core.Utils;

namespace lab1.Core;

public sealed class FastFoodSimulator
{
    public TimeSpan ArrivalInterval { get; }

    public TimeSpan OrderingInterval { get; }

    public TimeSpan PrepInterval { get; }

    private readonly IOrderQueue<Customer> _arrivals = new AsyncOrderQueue<Customer>();
    private readonly IOrderQueue<Order> _kitchen = new AsyncOrderQueue<Order>();
    private readonly IOrderQueue<PreparedOrder> _service = new AsyncOrderQueue<PreparedOrder>();
    private readonly CustomerFactory _customerFactory = new();
    private readonly OrderNumberGenerator _numbers = new();

    private readonly ServingLine _servingLine = new();

    private readonly WorkerPool<OrderTaker> _orderTakerPool;
    private readonly WorkerPool<Cook> _cookPool;

    public IReadOnlyList<OrderTaker> OrderTakers { get; }

    public IReadOnlyList<Cook> Cooks { get; }

    public Server Server { get; }

    public IReadOnlyOrderQueue<Customer> Arrivals => _arrivals;

    public IReadOnlyOrderQueue<Order> Kitchen => _kitchen;

    public IReadOnlyOrderQueue<PreparedOrder> Service => _service;

    public int ServingLineCount => _servingLine.Count;

    public IReadOnlyList<Customer> ServingLineCustomers => _servingLine.Snapshot();

    public FastFoodSimulator(
        TimeSpan arrivalInterval,
        TimeSpan orderingInterval,
        TimeSpan prepInterval,
        int orderTakerCount,
        int cookCount)
    {
        ArrivalInterval = arrivalInterval;
        OrderingInterval = orderingInterval;
        PrepInterval = prepInterval;

        OrderTakers = Enumerable
            .Range(0, orderTakerCount)
            .Select(_ => new OrderTaker(_kitchen, _servingLine, _numbers, orderingInterval))
            .ToArray();

        Cooks = Enumerable
            .Range(0, cookCount)
            .Select(_ => new Cook(_service, prepInterval))
            .ToArray();

        Server = new Server(_service, _servingLine);

        _orderTakerPool = new WorkerPool<OrderTaker>(OrderTakers);
        _cookPool = new WorkerPool<Cook>(Cooks);
    }

    private Customer ArriveCustomer(Customer customer)
    {
        _arrivals.Enqueue(customer);
        return customer;
    }

    public Customer ArriveRandomCustomer() => ArriveCustomer(_customerFactory.Create());

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var workers = new[]
        {
            IgnoreCancellation(DispatchAsync(
                _arrivals,
                _orderTakerPool,
                (taker, customer, token) => taker.ProcessAsync(customer, token),
                cancellationToken)),
            IgnoreCancellation(DispatchAsync(
                _kitchen,
                _cookPool,
                (cook, order, token) => cook.ProcessAsync(order, token),
                cancellationToken)),
            IgnoreCancellation(Server.RunAsync(cancellationToken)),
        };

        try
        {
            while (true)
            {
                await Task.Delay(ArrivalInterval, cancellationToken);
                ArriveRandomCustomer();
            }
        }
        catch (OperationCanceledException)
        {
        }

        await Task.WhenAll(workers);
    }

    private static async Task DispatchAsync<TWorker, TItem>(
        IOrderQueue<TItem> queue,
        WorkerPool<TWorker> pool,
        Func<TWorker, TItem, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
        where TWorker : class
    {
        var inFlight = new List<Task>();

        try
        {
            while (true)
            {
                var item = await queue.DequeueAsync(cancellationToken);
                var worker = await pool.AcquireAsync(cancellationToken);

                inFlight.RemoveAll(task => task.IsCompleted);
                inFlight.Add(ProcessAsync(worker, item, pool, handler, cancellationToken));
            }
        }
        catch (OperationCanceledException)
        {
        }

        await Task.WhenAll(inFlight);
    }

    private static async Task ProcessAsync<TWorker, TItem>(
        TWorker worker,
        TItem item,
        WorkerPool<TWorker> pool,
        Func<TWorker, TItem, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
        where TWorker : class
    {
        try
        {
            await handler(worker, item, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            pool.Release(worker);
        }
    }

    private static async Task IgnoreCancellation(Task task)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
    }
}
