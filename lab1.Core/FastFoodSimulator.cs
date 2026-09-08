using lab1.Core.Models;
using lab1.Core.Async;
using lab1.Core.Utils;

namespace lab1.Core;

public sealed class FastFoodSimulator
{
    public TimeSpan ArrivalInterval { get; }
    public TimeSpan PrepInterval { get; }

    private readonly IOrderQueue<Customer> _arrivals = new AsyncOrderQueue<Customer>();
    private readonly IOrderQueue<Order> _kitchen = new AsyncOrderQueue<Order>();
    private readonly IOrderQueue<PreparedOrder> _service = new AsyncOrderQueue<PreparedOrder>();
    private readonly CustomerFactory _customerFactory = new();
    private readonly OrderNumberGenerator _numbers = new();
    
    private readonly ServingLine _servingLine = new();

    public OrderTaker OrderTaker { get; }

    public Cook Cook { get; }

    public Server Server { get; }

    public FastFoodSimulator(TimeSpan arrivalInterval, TimeSpan prepInterval)
    {
        ArrivalInterval = arrivalInterval;
        PrepInterval = prepInterval;

        OrderTaker = new OrderTaker(_arrivals, _kitchen, _servingLine, _numbers);
        Cook = new Cook(_kitchen, _service, prepInterval);
        Server = new Server(_service, _servingLine);
    }

    private Customer ArriveCustomer(Customer customer)
    {
        _arrivals.Enqueue(customer);
        return customer;
    }

    public Customer ArriveRandomCustomer() => ArriveCustomer(_customerFactory.Create());

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var workers = new[]
        {
            IgnoreCancellation(OrderTaker.RunAsync(cancellationToken)),
            IgnoreCancellation(Cook.RunAsync(cancellationToken)),
            IgnoreCancellation(Server.RunAsync(cancellationToken)),
        };

        try
        {
            while (true)
            {
                await Task.Delay(ArrivalInterval, cancellationToken);
                ArriveRandomCustomer();
            }
        }
        catch (OperationCanceledException)
        {
        }

        await Task.WhenAll(workers);
    }

    private static async Task IgnoreCancellation(Task task)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
    }
}
