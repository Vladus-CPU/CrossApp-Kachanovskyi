
namespace Core.Reports;

public static class ReportExtensions
{
    public static IEnumerable<T> TopBy<T>(this IEnumerable<T> source, Func<T, decimal> valueSelector, Func<T, string> keySelector, int n)
    {
        return source.OrderByDescending(valueSelector)
            .ThenBy(keySelector, StringComparer.Ordinal)
            .Take(Math.Max(0, n));
    }
}
