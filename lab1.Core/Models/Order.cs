namespace lab1.Core.Models;

public enum OrderStage
{
    Queued,
    Preparing,
    Ready,
    Served,
}

public class Order
{
    public int Number { get; }

    public string CustomerName { get; }

    public string CustomerSurname { get; }

    public OrderStage Stage { get; internal set; }

    public Order(int number, string customerName, string customerSurname)
    {
        Number = number;
        CustomerName = customerName;
        CustomerSurname = customerSurname;
        Stage = OrderStage.Queued;
    }

    public override string ToString() => $"#{Number} ({CustomerName} {CustomerSurname}) [{Stage}]";
}
