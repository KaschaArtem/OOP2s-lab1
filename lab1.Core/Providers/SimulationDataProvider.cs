namespace lab1.Core.Providers;

public sealed class SimulationDataProvider
{
    private readonly FastFoodSimulator _simulator;

    public SimulationDataProvider(FastFoodSimulator simulator)
    {
        _simulator = simulator;
    }

    public event EventHandler<SimulationSnapshot>? Updated;

    public SimulationSnapshot Capture()
    {
        var kitchenWaiting = _simulator.Kitchen.Snapshot();

        var orderTakers = _simulator.OrderTakers
            .Select(taker => new WorkerSnapshot(
                taker.CurrentOrder?.Number,
                taker.CurrentCustomer?.Name,
                taker.CurrentCustomer?.Surname))
            .ToArray();

        var cooks = _simulator.Cooks
            .Select(cook => new WorkerSnapshot(
                cook.CurrentOrder?.Number,
                cook.CurrentOrder?.CustomerName,
                cook.CurrentOrder?.CustomerSurname))
            .ToArray();

        return new SimulationSnapshot(
            OrderLineCount: _simulator.Arrivals.Count,
            OrderLineCustomers: _simulator.Arrivals.Snapshot(),
            OrderTakers: orderTakers,
            Cooks: cooks,
            KitchenWaitingOrders: kitchenWaiting,
            KitchenWaitingCount: kitchenWaiting.Count,
            PickupWaitingCustomers: _simulator.ServingLineCustomers,
            ServingLineCount: _simulator.ServingLineCount);
    }

    public async Task StreamUpdatesAsync(TimeSpan interval, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                Updated?.Invoke(this, Capture());
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
