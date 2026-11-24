using Api.Contracts;
using Api.Validation;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using OrderDto = Api.Contracts.OrderDto;

namespace Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrders(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapPost("/", Create).AddEndpointFilter<OrderValidationFilter>().ProducesValidationProblem().ProducesProblem(409);
        group.MapPut("/{id}", Update).AddEndpointFilter<OrderValidationFilter>().ProducesValidationProblem().ProducesProblem(409);
        group.MapDelete("/{id}", Delete);
        group.MapGet("/{id}/total", GetTotal);
    }

    private static async Task<Results<Ok<OrderPage>, ValidationProblem>> GetAll(IOrderStore store, CancellationToken ct, int page = 1, int pageSize = 20, string? search = null)
    {
        var errors = new Dictionary<string, string[]>();

        if (page < 1)
            errors["page"] = ["Номер сторінки має бути більшим за нуль."];

        if (pageSize < 1 || pageSize > 100)
            errors["pageSize"] = ["Розмір сторінки має бути від 1 до 100."];

        if (errors.Count > 0)
            return TypedResults.ValidationProblem(errors);

        var orders = await store.ListAsync(ct);
        var query = orders.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(order => order.Id.Contains(search, StringComparison.OrdinalIgnoreCase) || order.CustomerId.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = query.OrderBy(order => order.Id).ToList();

        int total = filtered.Count;
        long skip = (long)(page - 1) * pageSize;

        var items = skip >= total ? new List<OrderDto>() : filtered.Skip((int)skip).Take(pageSize).Select(OrderDto.FromOrder).ToList();

        return TypedResults.Ok(new OrderPage(items, total));
    }

    private static async Task<Results<Ok<OrderDto>, NotFound>> GetById(string id, IOrderStore store, CancellationToken ct)
    {
        var order = await store.GetByIdAsync(id, ct);

        if (order is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(OrderDto.FromOrder(order));
    }

    private static async Task<Created<OrderDto>> Create(SaveOrderRequest request, IOrderStore store, CancellationToken ct)
    {
        var order = Order.Create(request.CustomerId!);

        foreach (var line in request.Lines!)
        {
            order.AddLine(line.ProductId!, line.Name!, line.Price!.Value, line.Quantity!.Value);
        }

        await store.AddAsync(order, ct);

        return TypedResults.Created($"/api/orders/{order.Id}", OrderDto.FromOrder(order));
    }

    private static async Task<Results<Ok<OrderDto>, NotFound>> Update(string id, SaveOrderRequest request, IOrderStore store, CancellationToken ct)
    {
        var existing = await store.GetByIdAsync(id, ct);

        if (existing is null)
            return TypedResults.NotFound();

        var lines = request.Lines!.Select(line => new OrderLineDto(line.ProductId!, line.Name!, line.Price!.Value, line.Quantity!.Value)).ToList();

        var dto = new Core.Dto.OrderDto(existing.Id, request.CustomerId!, lines, OrderStatus.Draft) { CreatedAt = existing.CreatedAt };

        var replacement = Order.FromDto(dto);

        if (!await store.TryUpdateAsync(replacement, ct))
            return TypedResults.NotFound();

        return TypedResults.Ok(OrderDto.FromOrder(replacement));
    }

    private static async Task<Results<NoContent, NotFound>> Delete(string id, IOrderStore store, CancellationToken ct)
    {
        if (!await store.RemoveAsync(id, ct))
            return TypedResults.NotFound();

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<OrderTotalDto>, NotFound>> GetTotal(string id, IOrderStore store, CancellationToken ct)
    {
        var order = await store.GetByIdAsync(id, ct);

        if (order is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(new OrderTotalDto(order.Id, order.Total));
    }
}