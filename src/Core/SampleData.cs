using Core.Domain;
using Core.Dto;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Order> Orders()
    {
        var orders = new List<Order>();

        for (int i = 1; i <= 15; i++)
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
}