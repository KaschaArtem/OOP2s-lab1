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

        return new SimulationSnapshot(
            OrderLineCount: _simulator.Arrivals.Count,
            OrderTakerOrderNumber: _simulator.OrderTaker.CurrentOrder?.Number,
            KitchenCurrentOrderNumber: _simulator.Cook.CurrentOrder?.Number,
            KitchenWaitingOrders: kitchenWaiting,
            KitchenWaitingCount: kitchenWaiting.Count,
            PickupOrderNumber: _simulator.Server.AnnouncedOrder?.Number,
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
