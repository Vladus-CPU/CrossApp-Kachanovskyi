using System.Text;
using Core.Domain;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

bool lab4Mode = args.Length == 0 ||
    args[0].Equals("--lab4", StringComparison.OrdinalIgnoreCase);

if (lab4Mode)
{
    RunLab4();
    return 0;
}

bool mixedMode = args[0].Equals("--mixed", StringComparison.OrdinalIgnoreCase);

string path;

if (mixedMode)
{
    path = args.Length > 1 ? args[1] : Path.Combine("data", "mixed.csv");
}
else
{
    path = args[0];
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

    Console.WriteLine(
        $"Статистика: усього {total} / " +
        $"прийнято {mixedResult.Items.Count} / " +
        $"пропущено {mixedResult.Errors.Count} / " +
        $"помилок {errorPercent:F1}%");

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

Console.WriteLine(
    $"Статистика: усього {allRecords} / " +
    $"прийнято {result.Items.Count} / " +
    $"пропущено {result.Errors.Count} / " +
    $"помилок {errorsPercent:F1}%");

return 0;

static void RunLab4()
{
    Console.WriteLine("=== Сценарій 1: успіх ===");

    Order order = Order.Create("C-001");
    order.AddLine("P-001", "Ноутбук", 28999.90m, 1);
    order.AddLine("P-002", "Миша", 649.50m, 2);

    Console.WriteLine(order);

    order.Confirm();
    Console.WriteLine(order);

    OrderDto dto = order.ToDto();
    Order restored = Order.FromDto(dto);

    Console.WriteLine($"Відновлено з DTO: {restored}");
    Console.WriteLine();

    Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");

    TryDo("додавання рядка після підтвердження",
        () => order.AddLine("P-003", "Клавіатура", 1299.00m, 1));

    TryDo("нульова кількість",
        () => Order.Create("C-002").AddLine("P-004", "Монітор", 7499.99m, 0));

    TryDo("від'ємна ціна",
        () => Order.Create("C-003").AddLine("P-005", "Навушники", -1m, 1));

    TryDo("підтвердження порожнього замовлення",
        () => Order.Create("C-004").Confirm());

    Console.WriteLine();
    Console.WriteLine($"Після відмов підтверджене замовлення не змінилось: {order}");
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"{title}: виняток НЕ спрацював");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{title}: {ex.GetType().Name} - {ex.Message}");
    }
}
