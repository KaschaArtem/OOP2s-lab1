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
            return _waiting
                .OrderBy(entry => entry.Key)
                .Select(entry => entry.Value)
                .ToList()
                .AsReadOnly();
        }
    }

    public void Add(int orderNumber, Customer customer)
    {
        lock (_gate)
        {
            _waiting[orderNumber] = customer;
        }
    }

    public bool Remove(int orderNumber)
    {
        lock (_gate)
        {
            return _waiting.Remove(orderNumber);
        }
    }
}
