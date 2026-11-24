namespace Core.Domain;

public static class OrderRules
{
    public static void CheckOrderLimit(Customer customer, IReadOnlyList<Order> orders)
    {
        CheckOrderLimit(customer.Id, orders);
    }

    public static void CheckOrderLimit(string customerId, IReadOnlyList<Order> orders)
    {
        int activeOrders = orders.Count(order => order.CustomerId == customerId && order.Status == OrderStatus.Draft);

        if (activeOrders >= 5)
            throw new DomainException($"Клієнт {customerId} вже має 5 активних замовлень");
    }

    public static void CheckReplacement(Order existing, Order replacement, IReadOnlyList<Order> orders)
    {
        if (existing.Status != OrderStatus.Draft)
            throw new DomainException($"Замовлення {existing.Id} не можна змінювати");

        if (existing.CustomerId != replacement.CustomerId)
            CheckOrderLimit(replacement.CustomerId, orders);
    }
}