namespace lab1.Core.Models;

public sealed class Server
{
    private readonly IOrderQueue<PreparedOrder> _service;
    private readonly ServingLine _servingLine;

    public Order? CurrentOrder { get; private set; }

    public Order? AnnouncedOrder { get; private set; }

    public int ServedCount { get; private set; }

    public Server(IOrderQueue<PreparedOrder> service, ServingLine servingLine)
    {
        _service = service;
        _servingLine = servingLine;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var prepared = await _service.DequeueAsync(cancellationToken);
            CurrentOrder = prepared.Order;

            await prepared.Preparation.WaitAsync(cancellationToken);

            prepared.Order.Stage = OrderStage.Served;
            AnnouncedOrder = prepared.Order;
            _servingLine.Remove(prepared.Order.Number)?.Announce();
            ServedCount++;
        }
    }
}
