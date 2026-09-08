namespace lab1.Core.Models;

public enum CustomerState
{
    InOrderLine,
    Ordering,
    InServingLine,
    Served,
}

public class Customer
{
    public string Name { get; }

    public string Surname { get; }

    public CustomerState State { get; internal set; }

    public Order? Receipt { get; internal set; }

    private readonly TaskCompletionSource<Order> _pickup =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Customer(string name, string surname)
    {
        Name = name;
        Surname = surname;
        State = CustomerState.InOrderLine;
    }

    internal void StartOrdering() => State = CustomerState.Ordering;

    internal void ReceiveReceipt(Order order)
    {
        Receipt = order;
        State = CustomerState.InServingLine;
    }

    internal void Announce()
    {
        if (Receipt is null)
            return;

        State = CustomerState.Served;
        _pickup.TrySetResult(Receipt);
    }

    public Task<Order> WaitForPickupAsync(CancellationToken cancellationToken = default)
        => _pickup.Task.WaitAsync(cancellationToken);

    public override string ToString()
        => $"{Name} {Surname} ({(Receipt is null ? "-" : Receipt.Number.ToString())})";
}
