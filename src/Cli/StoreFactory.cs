using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;

public static class StoreFactory
{
    public static IOrderStore Create(string[] args, string? storagePath = null)
    {
        bool useFile = args.Any(arg => arg.Equals("--file", StringComparison.OrdinalIgnoreCase));

        bool useCache = args.Any(arg => arg.Equals("--cache", StringComparison.OrdinalIgnoreCase));

        string path = storagePath ?? Path.Combine(AppContext.BaseDirectory, "data", "orders-store.json");

        IOrderStore store = useFile
            ? new FileOrderStore(path)
            : new InMemoryOrderStore(SampleData.Orders());

        if (useCache)
            store = new CachingOrderStore(store);

        return store;
    }
}