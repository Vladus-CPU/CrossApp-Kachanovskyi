using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;
    private readonly INotificationSink? _sink;

    public OrderService(IOrderStore store, INotificationSink? sink = null)
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

    public async Task<int> ImportAsync(IOrderImporter importer, string path, CancellationToken ct = default, IProgress<int>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(importer);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ct.ThrowIfCancellationRequested();

        var orders = await importer.LoadAsync(path, ct);
        int added = 0;

        if (orders.Count == 0)
            progress?.Report(100);

        for (int i = 0; i < orders.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            Order order = orders[i];

            if (_store.GetById(order.Id) is null)
            {
                _store.Add(order);
                OnChanged(order, OrderChangeKind.OrderCreated);
                added++;
            }

            progress?.Report((i + 1) * 100 / orders.Count);
        }

        return added;
    }

    public async Task<int> ImportStreamingAsync(IOrderImporter importer, string path, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(importer);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ct.ThrowIfCancellationRequested();

        int added = 0;

        await foreach (Order order in importer.StreamAsync(path, ct).WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();

            if (_store.GetById(order.Id) is not null)
                continue;

            _store.Add(order);
            OnChanged(order, OrderChangeKind.OrderCreated);
            added++;
        }

        return added;
    }

    private Order GetRequired(string id) =>
        _store.GetById(id) ?? throw new DomainException($"Замовлення з id={id} не знайдено.");
}