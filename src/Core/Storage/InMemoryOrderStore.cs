using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly Dictionary<string, Order> _orders;

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
        return _orders.Values.ToList();
    }

    public Order? GetById(string id)
    {
        return _orders.GetValueOrDefault(id);
    }

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (_orders.ContainsKey(order.Id))
            throw new InvalidOperationException($"Замовлення з id={order.Id} уже існує.");

        _orders.Add(order.Id, order);
    }

    public void Update(Order order)
    {
        _orders[order.Id] = order;
    }

    public bool Remove(string id)
    {
        return _orders.Remove(id);
    }
}