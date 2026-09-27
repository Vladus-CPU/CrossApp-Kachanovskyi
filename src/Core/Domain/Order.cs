using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public decimal Total => _lines.Sum(line => line.Price * line.Quantity);

    private Order(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
    }

    public static Order Create(string customerId)
    {
        return Create(Guid.NewGuid().ToString("N"), customerId);
    }

    private static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення не може бути порожнім", nameof(id));

        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Ідентифікатор клієнта не може бути порожнім", nameof(customerId));

        return new Order(id.Trim(), customerId.Trim());
    }

    public void AddLine(string productId, string name, decimal price, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Замовлення {Id} не можна змінювати у стані {Status}");

        OrderLine line = OrderLine.Create(productId, name, price, quantity);
        _lines.Add(line);
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException($"Замовлення {Id} порожнє, його не можна підтвердити");

        Status = Status switch
        {
            OrderStatus.Draft => OrderStatus.Confirmed,
            OrderStatus.Confirmed => throw new InvalidOperationException($"Замовлення {Id} вже підтверджене"),
            OrderStatus.Cancelled => throw new InvalidOperationException($"Скасоване замовлення {Id} не можна підтвердити"),
            _ => throw new InvalidOperationException("Невідомий стан замовлення")
        };
    }

    public void Cancel()
    {
        Status = Status switch
        {
            OrderStatus.Draft => OrderStatus.Cancelled,
            OrderStatus.Confirmed => throw new InvalidOperationException($"Підтверджене замовлення {Id} не можна скасувати"),
            OrderStatus.Cancelled => throw new InvalidOperationException($"Замовлення {Id} вже скасоване"),
            _ => throw new InvalidOperationException("Невідомий стан замовлення")
        };
    }

    public OrderDto ToDto() => new(Id, CustomerId, _lines.Select(line => line.ToDto()).ToList(), Status);

    public static Order FromDto(OrderDto dto)
{
    if (dto is null)
        throw new ArgumentNullException(nameof(dto));

    if (dto.Lines is null)
        throw new ArgumentException("Список рядків замовлення не може бути null", nameof(dto));

    Order order = Create(dto.Id, dto.CustomerId);

    foreach (OrderLineDto line in dto.Lines)
        order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);

    switch (dto.Status)
    {
        case OrderStatus.Draft:
            break;

        case OrderStatus.Confirmed:
            order.Confirm();
            break;

        case OrderStatus.Cancelled:
            order.Cancel();
            break;
    }

    return order;
}

    public override string ToString() => $"{Id} клієнт {CustomerId}, рядків: {_lines.Count}, сума: {Total:F2}, стан: {Status}";
}