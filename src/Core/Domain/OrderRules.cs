namespace Core.Domain;

public static class OrderRules
{
    public static void CheckOrderLimit(Customer customer, IReadOnlyList<Order> orders)
    {
        int activeOrders = orders.Count(order => order.CustomerId == customer.Id && order.Status == OrderStatus.Draft);

        if (activeOrders >= 5)
            throw new InvalidOperationException($"Клієнт {customer.Id} вже має 5 активних замовлень");
    }
}