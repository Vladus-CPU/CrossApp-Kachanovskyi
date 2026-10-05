using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileOrderStore(string path) : IOrderStore
{
    private static readonly JsonSerializerOptions Options = new() { 
        WriteIndented = true 
    };

    private readonly Dictionary<string, Order> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded)
            return;

        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);
            List<OrderDto> dtos = JsonSerializer.Deserialize<List<OrderDto>>(json) ?? [];

            foreach (OrderDto dto in dtos)
            {
                Order order = Order.FromDto(dto);
                _cache[order.Id] = order;
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

        List<OrderDto> dtos = _cache.Values.Select(order => order.ToDto()).ToList();
        string json = JsonSerializer.Serialize(dtos, Options);

        File.WriteAllText(_path, json);
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
            throw new InvalidOperationException($"Замовлення з id={order.Id} уже існує.");

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