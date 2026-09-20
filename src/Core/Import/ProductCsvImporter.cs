using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (line.Equals("id;name;price", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;

                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<ProductDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(
            Separator,
            StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 3 }
                => new ParseFailed(
                    $"очікую 3 колонки, отримав {parts.Length}"),

            ["", _, _] or [_, "", _]
                => new ParseFailed(
                    "ID або назва порожні"),

            [var id, var name, var priceText]
                when decimal.TryParse(
                    priceText,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal price) && price >= 0
                => new ParseOk(
                    new ProductDto(id, name, price)),

            [_, _, var priceText]
                => new ParseFailed(
                    $"ціна '{priceText}' не є невід'ємним числом"),

            _ => new ParseFailed(
                $"занадто багато колонок: {parts.Length}")
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseOk(ProductDto Value) : ParseOutcome;

    private sealed record ParseFailed(string Reason) : ParseOutcome;
}