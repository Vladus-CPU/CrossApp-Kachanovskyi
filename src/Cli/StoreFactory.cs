using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;

public static class StoreFactory
{
    public static IOrderStore Create(string[] args)
    {
        bool useFile = args.Any(arg => arg.Equals("--file", StringComparison.OrdinalIgnoreCase));
        bool useCache = args.Any(arg => arg.Equals("--cache", StringComparison.OrdinalIgnoreCase));

        string dataPath = Path.Combine("data", "orders.json");

        IOrderStore store = useFile
            ? new FileOrderStore(dataPath)
            : new InMemoryOrderStore(SampleData.Orders());

        if (useCache)
            store = new CachingOrderStore(store);

        return store;
    }
}