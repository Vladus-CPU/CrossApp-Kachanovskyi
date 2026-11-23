namespace Core.Configuration;

public sealed class AppOptions
{
    public string EnvironmentName { get; init; } = "Development";
    public StorageOptions Storage { get; init; } = new();
    public LoggingOptions Logging { get; init; } = new();
    public OrdersOptions Orders { get; init; } = new();
}

public sealed class StorageOptions
{
    public string DataPath { get; init; } = "data";
    public string FileName { get; init; } = "orders-store.json";

    public string ResolveFullPath(string baseDir)
    {
        if (string.IsNullOrWhiteSpace(FileName) ||
            Path.GetFileName(FileName) != FileName)
        {
            throw new ArgumentException(
                "FileName має містити лише назву файлу.");
        }

        string folder = Path.IsPathRooted(DataPath)
            ? DataPath
            : Path.Combine(baseDir, DataPath);

        Directory.CreateDirectory(folder);

        return Path.GetFullPath(Path.Combine(folder, FileName));
    }
}

public sealed class LoggingOptions
{
    public string MinimumLevel { get; init; } = "Information";
}

public sealed class OrdersOptions
{
    public string Currency { get; init; } = "UAH";
}