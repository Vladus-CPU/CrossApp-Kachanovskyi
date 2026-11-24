using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly Dictionary<string, Order> _orders;
    private readonly object _lock = new();

    public InMemoryOrderStore(IEnumerable<Order>? seed = null)
    {
        _orders = new Dictionary<string, Order>(StringComparer.OrdinalIgnoreCase);

        if (seed != null)
        {
            foreach (Order order in seed)
                _orders.Add(order.Id, order);
        }
    }

    public IReadOnlyList<Order> List()
    {
        lock (_lock)
        {
            return _orders.Values.ToList();
        }
    }

    public Order? GetById(string id)
    {
        lock (_lock)
        {
            return _orders.GetValueOrDefault(id);
        }
    }

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        lock (_lock)
        {
            if (_orders.ContainsKey(order.Id))
                throw new InvalidOperationException($"Замовлення з id={order.Id} уже існує.");

            _orders.Add(order.Id, order);
        }
    }

    public void Update(Order order)
    {
        lock (_lock)
        {
            _orders[order.Id] = order;
        }
    }

    public bool Remove(string id)
    {
        lock (_lock)
        {
            return _orders.Remove(id);
        }
    }

    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_lock)
        {
            OrderRules.CheckOrderLimit(order.CustomerId, List());
            Add(order);
        }

        return Task.CompletedTask;
    }

    public Task<bool> TryUpdateAsync(Order order, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_lock)
        {
            if (!_orders.TryGetValue(order.Id, out var existing))
                return Task.FromResult(false);

            OrderRules.CheckReplacement(existing, order, List());
            Update(order);

            return Task.FromResult(true);
        }
    }
}