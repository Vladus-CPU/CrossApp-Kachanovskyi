using System.Text;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Import;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

if (args.Length > 0 && args[0].Equals("--order", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("=== Успішний сценарій ===");

    Order order = Order.Create("C-001");
    order.AddLine("P-001", "Ноутбук", 28999.90m, 1);
    order.AddLine("P-002", "Миша", 649.50m, 2);

    Console.WriteLine(order);

    order.Confirm();
    Console.WriteLine(order);

    OrderDto dto = order.ToDto();
    Order restoredOrder = Order.FromDto(dto);

    Console.WriteLine($"Відновлено з DTO: {restoredOrder}");
    Console.WriteLine();

    Console.WriteLine("=== Порушення інваріантів ===");

    try
    {
        Order.Create("");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Порожній ID клієнта: {ex.Message}");
    }

    try
    {
        Order testOrder = Order.Create("C-002");
        testOrder.AddLine("P-003", "Клавіатура", 1299.00m, 0);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Неправильна кількість: {ex.Message}");
    }

    try
    {
        Order testOrder = Order.Create("C-003");
        testOrder.AddLine("P-004", "Монітор", -100m, 1);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Неправильна ціна: {ex.Message}");
    }

    try
    {
        Order emptyOrder = Order.Create("C-004");
        emptyOrder.Confirm();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Порожнє замовлення: {ex.Message}");
    }

    try
    {
        order.AddLine("P-005", "Навушники", 2199.50m, 1);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Зміна підтвердженого замовлення: {ex.Message}");
    }

    try
    {
        order.Confirm();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Повторне підтвердження: {ex.Message}");
    }

    Order cancelledOrder = Order.Create("C-005");
    cancelledOrder.AddLine("P-006", "SSD", 2999.99m, 1);
    cancelledOrder.Cancel();

    try
    {
        cancelledOrder.Confirm();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Підтвердження скасованого: {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("=== ImportResult ===");

    var orderDtos = new List<OrderDto>
    {
        order.ToDto(),
        new OrderDto("O-002", "", [], OrderStatus.Draft),
        new OrderDto("O-003", "C-003", [], OrderStatus.Confirmed)
    };

    var importResult = new ImportResult<OrderDto>(orderDtos, []);
    ImportResult<Order> importedOrders = OrderMapper.FromImportResult(importResult);

    Console.WriteLine($"Прийнято замовлень: {importedOrders.Items.Count}");
    Console.WriteLine($"Помилок: {importedOrders.Errors.Count}");

    foreach (string error in importedOrders.Errors)
        Console.WriteLine($"! {error}");

    Console.WriteLine();
    Console.WriteLine("=== Правило між сутностями ===");

    Customer customer = Customer.Create("C-100", "Іван Петренко", "ivan@example.com");
    var customerOrders = new List<Order>();

    for (int i = 0; i < 5; i++)
        customerOrders.Add(Order.Create(customer.Id));

    try
    {
        OrderRules.CheckOrderLimit(customer, customerOrders);
        customerOrders.Add(Order.Create(customer.Id));
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

    return 0;
}

bool storeMode = args.Length == 0 ||
    args.Contains("--store") ||
    args.Contains("--file") ||
    args.Contains("--cache");

if (storeMode)
{
    RunOrderStorage(args);
    return 0;
}

bool mixedMode = args.Length > 0 && args[0].Equals("--mixed", StringComparison.OrdinalIgnoreCase);

string path;

if (mixedMode)
{
    path = args.Length > 1 ? args[1] : Path.Combine("data", "mixed.csv");
}
else
{
    path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");
}

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

if (mixedMode)
{
    ImportResult<ImportEntryDto> mixedResult = MixedCsvImporter.Load(path);

    Console.WriteLine($"Завантажено записів: {mixedResult.Items.Count}");

    foreach (ImportEntryDto item in mixedResult.Items.Take(5))
    {
        switch (item)
        {
            case ProductDto product:
                Console.WriteLine($"Товар: {product.Id} {product.Name} {product.Price:F2}");
                break;

            case CustomerDto customer:
                Console.WriteLine($"Клієнт: {customer.Id} {customer.Name} {customer.Email}");
                break;
        }
    }

    if (mixedResult.Errors.Count > 0)
    {
        Console.WriteLine($"Пропущено рядків: {mixedResult.Errors.Count}");

        foreach (string error in mixedResult.Errors)
            Console.WriteLine($"! {error}");
    }

    int total = mixedResult.Items.Count + mixedResult.Errors.Count;
    double errorPercent = total == 0 ? 0 : mixedResult.Errors.Count * 100.0 / total;

    Console.WriteLine($"Статистика: усього {total} / прийнято {mixedResult.Items.Count} / пропущено {mixedResult.Errors.Count} / помилок {errorPercent:F1}%");

    return 0;
}

string extension = Path.GetExtension(path).ToLowerInvariant();

ImportResult<ProductDto>? result = extension switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    _ => null
};

if (result is null)
{
    Console.WriteLine($"Непідтримуваний формат файлу: {extension}");
    return 1;
}

Console.WriteLine($"Завантажено записів: {result.Items.Count}");

foreach (ProductDto product in result.Items.Take(5))
    Console.WriteLine($"{product.Id,-6} {product.Name,-24} {product.Price:F2}");

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
        Console.WriteLine($"! {error}");
}

int allRecords = result.Items.Count + result.Errors.Count;
double errorsPercent = allRecords == 0 ? 0 : result.Errors.Count * 100.0 / allRecords;

Console.WriteLine($"Статистика: усього {allRecords} / прийнято {result.Items.Count} / пропущено {result.Errors.Count} / помилок {errorsPercent:F1}%");

return 0;

static void RunOrderStorage(string[] args)
{
    IOrderStore store = StoreFactory.Create(args);
    var service = new OrderService(store);

    Console.WriteLine($"Сховище: {store.GetType().Name}");
    Console.WriteLine();

    try
    {
        Customer customer = Customer.Create("C-100", "Іван Петренко", "ivan@example.com");

        Order created = service.CreateOrder(customer);
        service.AddLine(created.Id, "P-101", "Клавіатура", 1299.00m, 1);
        service.AddLine(created.Id, "P-102", "Миша", 649.50m, 2);
        service.Confirm(created.Id);

        Console.WriteLine($"Створено замовлення: {created.Id}");

        Console.WriteLine();
        Console.WriteLine("=== Список замовлень ===");

        foreach (Order order in service.All())
            Console.WriteLine(order);

        Console.WriteLine();
        Console.WriteLine("=== Пошук за id ===");

        Order? found = service.Find(created.Id);

        if (found is not null)
            Console.WriteLine($"Знайдено: {found}");

        Console.WriteLine();
        Console.WriteLine("=== Підтверджені замовлення ===");

        IReadOnlyList<Order> confirmedOrders = service.Search(order => order.Status == OrderStatus.Confirmed);

        foreach (Order order in confirmedOrders)
            Console.WriteLine(order);

        Console.WriteLine();
        Console.WriteLine("=== Перевірка помилки ===");

        service.Confirm("ORDER-NOT-FOUND");
    }
    catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
    {
        Console.WriteLine($"Помилка: {ex.Message}");
    }
}

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