using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingOrderStore(IOrderStore inner) : IOrderStore
{
    private readonly IOrderStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private Dictionary<string, Order>? _cache;

    private void EnsureLoaded()
    {
        if (_cache is not null)
            return;

        _cache = _inner.List().ToDictionary(order => order.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<Order> List()
    {
        EnsureLoaded();
        return _cache!.Values.ToList();
    }

    public Order? GetById(string id)
    {
        EnsureLoaded();
        return _cache!.GetValueOrDefault(id);
    }

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        EnsureLoaded();

        _inner.Add(order);
        _cache!.Add(order.Id, order);
    }

    public void Update(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        EnsureLoaded();

        _inner.Update(order);
        _cache![order.Id] = order;
    }

    public bool Remove(string id)
    {
        EnsureLoaded();

        if (!_inner.Remove(id))
            return false;

        _cache!.Remove(id);
        return true;
    }
}