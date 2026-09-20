using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<ImportEntryDto> Load(string path)
    {
        var items = new List<ImportEntryDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (line.Equals(
                "type;id;name;value",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

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

        return new ImportResult<ImportEntryDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(
            Separator,
            StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 4 }
                => new ParseFailed(
                    $"очікую 4 колонки, отримав {parts.Length}"),

            ["P", "", _, _] or ["P", _, "", _]
                => new ParseFailed(
                    "ID або назва товару порожні"),

            ["P", var id, var name, var priceText]
                when decimal.TryParse(
                    priceText,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal price) && price >= 0
                => new ParseOk(
                    new ProductDto(id, name, price)),

            ["P", _, _, var priceText]
                => new ParseFailed(
                    $"ціна '{priceText}' не є невід'ємним числом"),

            ["C", "", _, _] or ["C", _, "", _]
                => new ParseFailed(
                    "ID або ім'я клієнта порожні"),

            ["C", var id, var name, var email]
                when email.Contains('@')
                => new ParseOk(
                    new CustomerDto(id, name, email)),

            ["C", _, _, var email]
                => new ParseFailed(
                    $"email '{email}' має неправильний формат"),

            [var type, _, _, _]
                => new ParseFailed(
                    $"невідомий тип запису '{type}'"),

            _ => new ParseFailed(
                $"занадто багато колонок: {parts.Length}")
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseOk(ImportEntryDto Value) : ParseOutcome;

    private sealed record ParseFailed(string Reason) : ParseOutcome;
}