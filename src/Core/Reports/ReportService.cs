
using System.Diagnostics;
using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    public static IReadOnlyList<CustomerSummary> TopCustomers(IEnumerable<Order> source, int n)
    {
        return source.GroupBy(o => o.CustomerId, StringComparer.OrdinalIgnoreCase)
            .Select(g => new CustomerSummary(g.Key, g.Count(), g.Sum(o => o.Total)))
            .TopBy(c => c.Total, c => c.CustomerId, n)
            .ToList();
    }

    public static IReadOnlyList<OrderReportLine> ExpensiveOrders(IEnumerable<Order> source, decimal threshold)
    {
        return source.Where(o => o.Total > threshold)
            .OrderByDescending(o => o.Total).ThenBy(o => o.Id, StringComparer.Ordinal)
            .Select(o => new OrderReportLine(o.Id, o.CustomerId, o.Total, o.Status))
            .ToList();
    }

    public static IReadOnlyList<GroupSummary> ByStatus(IEnumerable<Order> source)
    {
        return source.GroupBy(o => o.Status)
            .Select(g => new GroupSummary(g.Key.ToString(), g.Count(), g.Sum(o => o.Total)))
            .OrderByDescending(g => g.Total).ThenBy(g => g.Key, StringComparer.Ordinal)
            .ToList();
    }

    public static (int Count, decimal Total, decimal Average, decimal Max) Totals(IEnumerable<Order> source)
    {
        var list = source.ToList();

        return list.Count == 0
            ? (0, 0m, 0m, 0m)
            : (list.Count, list.Sum(o => o.Total), list.Average(o => o.Total), list.Max(o => o.Total));
    }

    public static IReadOnlyList<OrderReportLine> TopOrders(IEnumerable<Order> source, int n)
    {
        return source.TopBy(o => o.Total, o => o.Id, n)
            .Select(o => new OrderReportLine(o.Id, o.CustomerId, o.Total, o.Status))
            .ToList();
    }

    public static IReadOnlyList<CustomerOrderSummary> CustomerOrders(IEnumerable<Customer> customers, IEnumerable<Order> orders)
    {
        return customers.GroupJoin(orders,
                c => c.Id,
                o => o.CustomerId,
                (c, group) => new CustomerOrderSummary(c.Id, c.Name, group.Count(), group.Sum(o => o.Total)),
                StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(c => c.Total).ThenBy(c => c.CustomerId, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<GroupSummary> ByStatusQuery(IEnumerable<Order> source)
    {
        var query = from order in source 
                    group order by order.Status into g 
                    select new GroupSummary(g.Key.ToString(), g.Count(), g.Sum(o => o.Total));

        return query.OrderByDescending(g => g.Total)
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .ToList();
    }

    public static (int Before, int After, int Snapshot, int Current) DeferredDemo(IEnumerable<Order> items)
    {
        var source = items.ToList();
        var query = source.Where(o => o.Total > 500m);

        int before = query.Count();

        Order order = Order.Create("C-DEMO");
        order.AddLine("P-DEMO", "Тестовий товар", 600m, 1);
        source.Add(order);

        int after = query.Count();

        var snapshot = query.ToList();
        source.Clear();

        return (before, after, snapshot.Count, query.Count());
    }

    public static (double WithoutList, double WithList, bool Equal) Benchmark(int count)
    {
        var orders = new List<Order>();

        for (int i = 1; i <= count; i++)
        {
            Order order = Order.Create($"C-{((i - 1) % 5) + 1:000}");
            order.AddLine($"P-{i:000000}", "Товар", 100m + (i % 50) * 20m, 1);
            orders.Add(order);
        }

        decimal Calculate(bool withList)
        {
            IEnumerable<Order> query = orders.Where(o => o.Total > 500m);

            if (withList)
                query = query.ToList();

            return query.Select(o => o.Total)
                .OrderByDescending(total => total)
                .Take(10)
                .Sum();
        }

        Calculate(false);
        Calculate(true);

        var watch = new Stopwatch();
        double withoutList = 0;
        double withList = 0;
        decimal first = 0;
        decimal second = 0;

        for (int i = 0; i < 5; i++)
        {
            watch.Restart();
            first = Calculate(false);
            watch.Stop();
            withoutList += watch.Elapsed.TotalMilliseconds;

            watch.Restart();
            second = Calculate(true);
            watch.Stop();
            withList += watch.Elapsed.TotalMilliseconds;
        }

        return (withoutList / 5, withList / 5, first == second);
    }
}
