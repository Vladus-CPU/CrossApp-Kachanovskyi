using System.Text;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

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
    Console.WriteLine(
        $"Файл не знайдено: {Path.GetFullPath(path)}");

    return 1;
}

if (mixedMode)
{
    ImportResult<ImportEntryDto> mixedResult =
        MixedCsvImporter.Load(path);

    Console.WriteLine(
        $"Завантажено записів: {mixedResult.Items.Count}");

    foreach (ImportEntryDto item in mixedResult.Items.Take(5))
    {
        switch (item)
        {
            case ProductDto product:
                Console.WriteLine(
                    $"Товар: {product.Id} {product.Name} {product.Price:F2}");
                break;

            case CustomerDto customer:
                Console.WriteLine(
                    $"Клієнт: {customer.Id} {customer.Name} {customer.Email}");
                break;
        }
    }

    if (mixedResult.Errors.Count > 0)
    {
        Console.WriteLine(
            $"Пропущено рядків: {mixedResult.Errors.Count}");

        foreach (string error in mixedResult.Errors)
        {
            Console.WriteLine($"! {error}");
        }
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
    Console.WriteLine(
        $"Непідтримуваний формат файлу: {extension}");

    return 1;
}

Console.WriteLine(
    $"Завантажено записів: {result.Items.Count}");

foreach (ProductDto product in result.Items.Take(5))
{
    Console.WriteLine(
        $"{product.Id,-6} {product.Name,-24} {product.Price:F2}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine(
        $"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
    {
        Console.WriteLine($"! {error}");
    }
}

int allRecords = result.Items.Count + result.Errors.Count;

double errorsPercent = allRecords == 0 ? 0 : result.Errors.Count * 100.0 / allRecords;

Console.WriteLine(
    $"Статистика: усього {allRecords} / " +
    $"прийнято {result.Items.Count} / " +
    $"пропущено {result.Errors.Count} / " +
    $"помилок {errorsPercent:F1}%");

return 0;