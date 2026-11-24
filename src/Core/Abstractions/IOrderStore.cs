using Core.Domain;

namespace Core.Abstractions;

public interface IOrderStore
{
    IReadOnlyList<Order> List();
    Order? GetById(string id);
    void Add(Order order);
    void Update(Order order);
    bool Remove(string id);

    Task<IReadOnlyList<Order>> ListAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(List());
    }

    Task<Order?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(GetById(id));
    }

    Task AddAsync(Order order, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        OrderRules.CheckOrderLimit(order.CustomerId, List());
        Add(order);
        return Task.CompletedTask;
    }

    Task<bool> TryUpdateAsync(Order order, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var existing = GetById(order.Id);

        if (existing is null)
            return Task.FromResult(false);

        OrderRules.CheckReplacement(existing, order, List());
        Update(order);

        return Task.FromResult(true);
    }

    Task<bool> RemoveAsync(string id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Remove(id));
    }
}