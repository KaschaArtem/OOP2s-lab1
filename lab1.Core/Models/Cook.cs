namespace lab1.Core.Models;

public sealed class Cook
{
    private readonly IOrderQueue<Order> _kitchen;
    private readonly IOrderQueue<PreparedOrder> _service;

    public TimeSpan PrepInterval { get; }

    public Order? CurrentOrder { get; private set; }

    public int PreparedCount { get; private set; }

    public Cook(
        IOrderQueue<Order> kitchen,
        IOrderQueue<PreparedOrder> service,
        TimeSpan prepInterval)
    {
        _kitchen = kitchen;
        _service = service;
        PrepInterval = prepInterval;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var order = await _kitchen.DequeueAsync(cancellationToken);
            CurrentOrder = order;
            order.Stage = OrderStage.Preparing;

            var preparation = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _service.Enqueue(new PreparedOrder(order, preparation.Task));

            await Task.Delay(PrepInterval, cancellationToken);

            order.Stage = OrderStage.Ready;
            PreparedCount++;
            preparation.SetResult();

            CurrentOrder = null;
        }
    }
}
