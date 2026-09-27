using Core.Domain;

namespace Core.Dto;

public sealed record OrderDto(
    string Id,
    string CustomerId,
    IReadOnlyList<OrderLineDto> Lines,
    OrderStatus Status
);