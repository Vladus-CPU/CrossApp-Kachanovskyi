using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileOrderStore(string path) : IOrderStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly Dictionary<string, Order> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded)
            return;

        if (!File.Exists(_path))
        {
            _loaded = true;
            return;
        }

        List<OrderDto> dtos;

        try
        {
            string json = File.ReadAllText(_path);

            dtos = JsonSerializer.Deserialize<List<OrderDto>>(
                json, Options)
                ?? throw new InvalidDataException(
                    "Очікується масив замовлень, а не null.");
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(
                $"Некоректний JSON у файлі {Path.GetFileName(_path)}: {ex.Message}",
                ex);
        }

        var loaded = new Dictionary<string, Order>(
            StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < dtos.Count; i++)
        {
            try
            {
                Order order = Order.FromDto(dtos[i]);
                loaded.Add(order.Id, order);
            }
            catch (Exception ex) when (
                ex is ArgumentException or DomainException)
            {
                throw new InvalidDataException(
                    $"Помилка в замовленні {i + 1}: {ex.Message}",
                    ex);
            }
        }

        foreach (var pair in loaded)
            _cache.Add(pair.Key, pair.Value);

        _loaded = true;
    }

    private void Flush()
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(_path)!);

        List<OrderDto> dtos = _cache.Values
            .Select(order => order.ToDto())
            .ToList();

        string json = JsonSerializer.Serialize(dtos, Options);
        string tmp = _path + ".tmp";

        try
        {
            File.WriteAllText(tmp, json);
            File.Move(tmp, _path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tmp))
                File.Delete(tmp);
        }
    }

    public void SaveAll(IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);

        var items = new Dictionary<string, Order>(
            StringComparer.OrdinalIgnoreCase);

        foreach (Order order in orders)
        {
            ArgumentNullException.ThrowIfNull(order);
            items.Add(order.Id, order);
        }

        _cache.Clear();

        foreach (var pair in items)
            _cache.Add(pair.Key, pair.Value);

        _loaded = true;
        Flush();
    }

    public IReadOnlyList<Order> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Order? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        EnsureLoaded();

        if (_cache.ContainsKey(order.Id))
            throw new InvalidOperationException(
                $"Замовлення з id={order.Id} уже існує.");

        _cache.Add(order.Id, order);
        Flush();
    }

    public void Update(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        EnsureLoaded();

        _cache[order.Id] = order;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();

        if (!_cache.Remove(id))
            return false;

        Flush();
        return true;
    }
}