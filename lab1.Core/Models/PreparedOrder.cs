namespace lab1.Core.Models;

public sealed class PreparedOrder
{
    public Order Order { get; }

    public Task Preparation { get; }

    internal PreparedOrder(Order order, Task preparation)
    {
        Order = order;
        Preparation = preparation;
    }
}
