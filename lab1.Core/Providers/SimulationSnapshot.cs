using lab1.Core.Models;

namespace lab1.Core.Providers;

public sealed record SimulationSnapshot(
    int OrderLineCount,
    int? OrderTakerOrderNumber,
    int? KitchenCurrentOrderNumber,
    IReadOnlyList<Order> KitchenWaitingOrders,
    int KitchenWaitingCount,
    int? PickupOrderNumber,
    int ServingLineCount);
