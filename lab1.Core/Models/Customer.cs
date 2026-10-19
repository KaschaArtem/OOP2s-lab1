namespace lab1.Core.Models;

public class Customer
{
    public string Name { get; }

    public string Surname { get; }

    public Order? Receipt { get; internal set; }

    public Customer(string name, string surname)
    {
        Name = name;
        Surname = surname;
    }

    internal void ReceiveReceipt(Order order)
    {
        Receipt = order;
    }

    public override string ToString()
        => $"{Name} {Surname} ({(Receipt is null ? "-" : Receipt.Number.ToString())})";
}
