namespace Core.Dto;

public sealed record OrderLineDto(
    string ProductId,
    string Name,
    decimal Price,
    int Quantity
);
