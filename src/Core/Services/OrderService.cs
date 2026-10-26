using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;
    private readonly INotificationSink? _sink;

    public OrderService(
        IOrderStore store,
        INotificationSink? sink = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _sink = sink;
    }

    public event EventHandler<OrderChangedEventArgs>? Changed;

    private void OnChanged(Order order, OrderChangeKind kind)
    {
        var change = new OrderChangedEventArgs(order.Id, kind, order.Lines.Count, order.Total);

        Changed?.Invoke(this, change);
        _sink?.Notify(change);
    }

    public Order CreateOrder(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        OrderRules.CheckOrderLimit(customer, _store.List());

        Order order = Order.Create(customer.Id);
        _store.Add(order);
        OnChanged(order, OrderChangeKind.OrderCreated);

        return order;
    }

    public void AddLine(string orderId, string productId, string name, decimal price, int quantity)
    {
        Order order = GetRequired(orderId);
        order.AddLine(productId, name, price, quantity);
        _store.Update(order);
        OnChanged(order, OrderChangeKind.LineAdded);
    }

    public void Confirm(string orderId)
    {
        Order order = GetRequired(orderId);
        order.Confirm();
        _store.Update(order);
        OnChanged(order, OrderChangeKind.OrderConfirmed);
    }

    public void Cancel(string orderId)
    {
        Order order = GetRequired(orderId);
        order.Cancel();
        _store.Update(order);
        OnChanged(order, OrderChangeKind.OrderCancelled);
    }

    public IReadOnlyList<Order> All() => _store.List();

    public Order? Find(string id) => _store.GetById(id);

    public bool Remove(string id)
    {
        Order? order = _store.GetById(id);

        if (order is null)
            return false;

        if (!_store.Remove(id))
            return false;

        OnChanged(order, OrderChangeKind.OrderRemoved);

        return true;
    }

    public IReadOnlyList<Order> Search(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    private Order GetRequired(string id) =>
        _store.GetById(id) ?? throw new InvalidOperationException($"Замовлення з id={id} не знайдено.");
}