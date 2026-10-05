using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class OrderService(IOrderStore store)
{
    private readonly IOrderStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public Order CreateOrder(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        OrderRules.CheckOrderLimit(customer, _store.List());

        Order order = Order.Create(customer.Id);
        _store.Add(order);

        return order;
    }

    public void AddLine(string orderId, string productId, string name, decimal price, int quantity)
    {
        Order order = GetRequired(orderId);
        order.AddLine(productId, name, price, quantity);
        _store.Update(order);
    }

    public void Confirm(string orderId)
    {
        Order order = GetRequired(orderId);
        order.Confirm();
        _store.Update(order);
    }

    public void Cancel(string orderId)
    {
        Order order = GetRequired(orderId);
        order.Cancel();
        _store.Update(order);
    }

    public IReadOnlyList<Order> All() => _store.List();

    public Order? Find(string id) => _store.GetById(id);

    public bool Remove(string id) => _store.Remove(id);

    public IReadOnlyList<Order> Search(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    private Order GetRequired(string id) =>
        _store.GetById(id) ?? throw new InvalidOperationException($"Замовлення з id={id} не знайдено.");
}