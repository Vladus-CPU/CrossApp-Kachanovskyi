using Core.Dto;

namespace Core.Domain;

public static class OrderMapper
{
    public static ImportResult<Order> FromImportResult(ImportResult<OrderDto> result)
    {
        var orders = new List<Order>();
        var errors = new List<string>(result.Errors);

        for (int i = 0; i < result.Items.Count; i++)
        {
            try
            {
                Order order = Order.FromDto(result.Items[i]);
                orders.Add(order);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                errors.Add($"запис {i + 1}: {ex.Message}");
            }
        }

        return new ImportResult<Order>(orders, errors);
    }
}