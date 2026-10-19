using lab1.Core.Models;

namespace lab1.Core.Providers;

public sealed record SimulationSnapshot(
    int OrderLineCount,
    IReadOnlyList<Customer> OrderLineCustomers,
    IReadOnlyList<WorkerSnapshot> OrderTakers,
    IReadOnlyList<WorkerSnapshot> Cooks,
    IReadOnlyList<Order> KitchenWaitingOrders,
    int KitchenWaitingCount,
    IReadOnlyList<Customer> PickupWaitingCustomers,
    int ServingLineCount);