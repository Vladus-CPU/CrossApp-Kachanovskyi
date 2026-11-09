using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class OrderServiceTests
{
    [Fact]
    public void AddLine_WithNegativeQuantity_Throws_AndKeepsState()
    {
        var store = new InMemoryOrderStore();
        var service = new OrderService(store);

        var customer = Customer.Create("C-001", "Іван", "ivan@example.com");
        var order = service.CreateOrder(customer);

        service.AddLine(order.Id, "P-001", "Кабель", 100m, 2);

        decimal total = order.Total;
        int count = order.Lines.Count;

        Assert.Throws<ArgumentOutOfRangeException>(() => service.AddLine(order.Id, "P-002", "Миша", 50m, -1));

        Assert.Equal(total, order.Total);
        Assert.Equal(count, order.Lines.Count);
    }

    [Fact]
    public void CreateOrder_RaisesChangedExactlyOnce()
    {
        var service = new OrderService(new InMemoryOrderStore());
        var events = new List<OrderChangedEventArgs>();

        service.Changed += (_, e) => events.Add(e);

        var customer = Customer.Create("C-002", "Олег", "oleg@example.com");
        service.CreateOrder(customer);

        Assert.Single(events);
        Assert.Equal(OrderChangeKind.OrderCreated, events[0].Kind);
    }

    [Fact]
    public void AddLine_ToConfirmedOrder_ThrowsDomainException()
    {
        var service = new OrderService(new InMemoryOrderStore());

        var customer = Customer.Create("C-003", "Андрій", "andriy@example.com");
        var order = service.CreateOrder(customer);

        service.AddLine(order.Id, "P-001", "Кабель", 100m, 1);
        service.Confirm(order.Id);

        Assert.Throws<DomainException>(() => service.AddLine(order.Id, "P-002", "Миша", 50m, 1));

        Assert.Single(order.Lines);
        Assert.Equal(100m, order.Total);
    }

    [Fact]
    public void AddLine_UpdatesOrderTotal()
    {
        var service = new OrderService(new InMemoryOrderStore());

        var customer = Customer.Create("C-004", "Марія", "maria@example.com");
        var order = service.CreateOrder(customer);

        service.AddLine(order.Id, "P-001", "Кабель", 100m, 2);
        service.AddLine(order.Id, "P-002", "Миша", 50m, 3);

        Assert.Equal(350m, order.Total);
        Assert.Equal(2, order.Lines.Count);
    }
}