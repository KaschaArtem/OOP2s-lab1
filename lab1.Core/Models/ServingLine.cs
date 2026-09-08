using System.Collections.ObjectModel;

namespace lab1.Core.Models;

public sealed class ServingLine
{
    private readonly object _gate = new();
    private readonly Dictionary<int, Customer> _waiting = new();

    public int Count
    {
        get { lock (_gate) return _waiting.Count; }
    }

    public ReadOnlyCollection<Customer> Snapshot()
    {
        lock (_gate)
        {
            return _waiting.Values.ToList().AsReadOnly();
        }
    }

    public void Add(int orderNumber, Customer customer)
    {
        lock (_gate)
        {
            _waiting[orderNumber] = customer;
        }
    }

    public Customer? Remove(int orderNumber)
    {
        lock (_gate)
        {
            if (_waiting.Remove(orderNumber, out var customer))
                return customer;
            return null;
        }
    }
}
