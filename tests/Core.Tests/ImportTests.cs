using System.Runtime.CompilerServices;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class ImportTests
{
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var service = new OrderService(new InMemoryOrderStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ImportAsync(new JsonOrderImporter(), "any.json", cts.Token));
    }

    [Fact]
    public async Task ImportAsync_InvalidOrder_ThrowsDomainException()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        string json = """
        [
          {
            "Id": "ORD-BAD",
            "CustomerId": "C-001",
            "Lines": [
              {
                "ProductId": "P-001",
                "Name": "Кабель",
                "Price": 100,
                "Quantity": 0
              }
            ],
            "Status": 0
          }
        ]
        """;

        try
        {
            await File.WriteAllTextAsync(path, json);

            var service = new OrderService(new InMemoryOrderStore());

            var ex = await Assert.ThrowsAsync<DomainException>(() => service.ImportAsync(new JsonOrderImporter(), path));

            Assert.Contains("Запис 1", ex.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ImportAsync_DoesNotAddDuplicates()
    {
        var store = new InMemoryOrderStore();
        var service = new OrderService(store);
        var importer = new FixedOrderImporter(TestData.Orders());

        int first = await service.ImportAsync(importer, "unused.json");
        int second = await service.ImportAsync(importer, "unused.json");

        Assert.Equal(3, first);
        Assert.Equal(0, second);
        Assert.Equal(3, store.List().Count);
    }

    [Fact]
    public async Task ImportStreamingAsync_AddsOrders()
    {
        var store = new InMemoryOrderStore();
        var service = new OrderService(store);
        var importer = new FixedOrderImporter(TestData.Orders());

        int added = await service.ImportStreamingAsync(importer, "unused.json");

        Assert.Equal(3, added);
        Assert.Equal(3, store.List().Count);
    }

    [Fact]
    public async Task ImportAsync_LinkedToken_StopsByTimeout()
    {
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(50));

        var service = new OrderService(new InMemoryOrderStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ImportAsync(new SlowOrderImporter(), "unused.json", linkedCts.Token));

        Assert.True(timeoutCts.IsCancellationRequested);
        Assert.False(userCts.IsCancellationRequested);
    }

    private sealed class FixedOrderImporter(IReadOnlyList<Order> orders) : IOrderImporter
    {
        public Task<IReadOnlyList<Order>> LoadAsync(string path, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(orders);
        }

        public async IAsyncEnumerable<Order> StreamAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (Order order in orders)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(1, ct);
                yield return order;
            }
        }
    }

    private sealed class SlowOrderImporter : IOrderImporter
    {
        public async Task<IReadOnlyList<Order>> LoadAsync(string path, CancellationToken ct = default)
        {
            await Task.Delay(Timeout.Infinite, ct);
            return [];
        }

        public IAsyncEnumerable<Order> StreamAsync(string path, CancellationToken ct = default)
        {
            throw new NotSupportedException();
        }
    }
}