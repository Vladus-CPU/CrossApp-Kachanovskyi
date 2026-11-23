using Cli.Notifications;
using Core.Abstractions;
using Core.Services;
using Core.Storage;

namespace Cli;

public static class CompositionRoot
{
    public static (OrderService Service, IOrderStore Store) Create(string[] args, string? storagePath = null)
    {
        IOrderStore store = StoreFactory.Create(args, storagePath);
        var service = new OrderService(store);

        return (service, store);
    }

    public static OrderService CreateConsoleDemoService()
    {
        return new OrderService(
            new InMemoryOrderStore(),
            new ConsoleSink());
    }

    public static OrderService CreateFileDemoService(string path)
    {
        return new OrderService(
            new InMemoryOrderStore(),
            new FileSink(path));
    }
}