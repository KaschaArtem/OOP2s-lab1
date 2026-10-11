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
            OrderLineCustomers: _simulator.Arrivals.Snapshot(),
            OrderTakerOrderNumber: _simulator.OrderTaker.CurrentOrder?.Number,
            OrderTakerCustomerName: _simulator.OrderTaker.CurrentCustomer is { } takerCustomer
                ? $"{takerCustomer.Name} {takerCustomer.Surname}"
                : null,
            KitchenCurrentOrderNumber: _simulator.Cook.CurrentOrder?.Number,
            KitchenCurrentCustomerName: _simulator.Cook.CurrentOrder is { } cookingOrder
                ? $"{cookingOrder.CustomerName} {cookingOrder.CustomerSurname}"
                : null,
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
