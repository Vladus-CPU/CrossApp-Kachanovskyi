using Core.Domain;
using Core.Dto;

namespace Core.Tests;

public static class TestData
{
    public static IReadOnlyList<Order> Orders()
    {
        return
        [
            Order.FromDto(new OrderDto(
                "ORD-B", "C-001",
                [new OrderLineDto("P-001", "Кабель", 200m, 1)],
                OrderStatus.Draft)),

            Order.FromDto(new OrderDto(
                "ORD-A", "C-002",
                [new OrderLineDto("P-002", "Миша", 200m, 1)],
                OrderStatus.Draft)),

            Order.FromDto(new OrderDto(
                "ORD-LOW", "C-003",
                [new OrderLineDto("P-003", "Гвинт", 50m, 1)],
                OrderStatus.Draft))
        ];
    }
}