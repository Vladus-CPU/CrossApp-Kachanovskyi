using Core.Domain;
using Core.Dto;

namespace Api.Contracts;

public sealed record OrderDto(string Id, string CustomerId, DateTimeOffset? CreatedAt, IReadOnlyList<OrderLineDto> Lines, string Status, decimal Total)
{
    public static OrderDto FromOrder(Order order)
    {
        return new OrderDto(order.Id, order.CustomerId, order.CreatedAt, order.Lines.Select(line => line.ToDto()).ToList(), order.Status.ToString(), order.Total);
    }
}

public sealed record OrderPage(IReadOnlyList<OrderDto> Items, int Total);

public sealed record OrderTotalDto(string Id, decimal Total);