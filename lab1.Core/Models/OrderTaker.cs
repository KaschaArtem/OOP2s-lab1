using lab1.Core.Utils;

namespace lab1.Core.Models;

public sealed class OrderTaker
{
    private readonly IOrderQueue<Customer> _arrivals;
    private readonly IOrderQueue<Order> _kitchen;
    private readonly ServingLine _servingLine;
    private readonly OrderNumberGenerator _numbers;

    public Customer? CurrentCustomer { get; private set; }

    public Order? CurrentOrder { get; private set; }

    public OrderTaker(
        IOrderQueue<Customer> arrivals,
        IOrderQueue<Order> kitchen,
        ServingLine servingLine,
        OrderNumberGenerator numbers)
    {
        _arrivals = arrivals;
        _kitchen = kitchen;
        _servingLine = servingLine;
        _numbers = numbers;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var customer = await _arrivals.DequeueAsync(cancellationToken);
            CurrentCustomer = customer;
            customer.StartOrdering();

            var order = new Order(_numbers.Next(), customer.Name, customer.Surname);
            CurrentOrder = order;

            customer.ReceiveReceipt(order);
            _servingLine.Add(order.Number, customer);
            _kitchen.Enqueue(order);
        }
    }
}
