
using Core.Domain;
using Core.Dto;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Order> Orders()
    {
        var orders = new List<Order>();

        for (int i = 1; i <= 20; i++)
        {
            string orderId = $"ORD-{i:000}";
            string customerId = $"C-{((i - 1) % 5) + 1:000}";

            var lines = new List<OrderLineDto>
            {
                new($"P-{i:000}", $"Товар {i}", 100 + i * 25, i % 3 + 1)
            };

            OrderStatus status = OrderStatus.Draft;

            if (i % 5 == 0)
                status = OrderStatus.Cancelled;
            else if (i % 3 == 0)
                status = OrderStatus.Confirmed;

            OrderDto dto = new(orderId, customerId, lines, status);
            orders.Add(Order.FromDto(dto));
        }

        return orders;
    }

    public static IReadOnlyList<Customer> Customers()
    {
        return
        [
            Customer.Create("C-001", "Клієнт 1", "customer1@example.com"),
            Customer.Create("C-002", "Клієнт 2", "customer2@example.com"),
            Customer.Create("C-003", "Клієнт 3", "customer3@example.com"),
            Customer.Create("C-004", "Клієнт 4", "customer4@example.com"),
            Customer.Create("C-005", "Клієнт 5", "customer5@example.com")
        ];
    }
}
