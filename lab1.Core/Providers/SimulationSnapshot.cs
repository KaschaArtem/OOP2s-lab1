using lab1.Core.Models;

namespace lab1.Core.Providers;

public sealed record SimulationSnapshot(
    int OrderLineCount,
    IReadOnlyList<Customer> OrderLineCustomers,
    int? OrderTakerOrderNumber,
    string? OrderTakerCustomerName,
    int? KitchenCurrentOrderNumber,
    string? KitchenCurrentCustomerName,
    IReadOnlyList<Order> KitchenWaitingOrders,
    int KitchenWaitingCount,
    IReadOnlyList<Customer> PickupWaitingCustomers,
    int ServingLineCount);
