using Core.Reports;

namespace Core.Tests;

public class ReportServiceTests
{
    [Theory]
    [InlineData(100, 2)]
    [InlineData(200, 0)]
    public void ExpensiveOrders_CountsOnlyAboveThreshold(int threshold, int expected)
    {
        var result = ReportService.ExpensiveOrders(TestData.Orders(), threshold);

        Assert.Equal(expected, result.Count);
    }

    [Fact]
    public void TopOrders_IsDeterministic()
    {
        var result = ReportService.TopOrders(TestData.Orders(), 2);

        Assert.Equal(
            new[] { "ORD-A", "ORD-B" },
            result.Select(o => o.OrderId));
    }

    [Fact]
    public void Totals_CalculatesCorrectly()
    {
        var result = ReportService.Totals(TestData.Orders());

        Assert.Equal(3, result.Count);
        Assert.Equal(450m, result.Total);
        Assert.Equal(150m, result.Average);
        Assert.Equal(200m, result.Max);
    }
}