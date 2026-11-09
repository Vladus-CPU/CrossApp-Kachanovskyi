using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonOrderImporter : IOrderImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<Order>> LoadAsync(string path, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

        var dtos = await JsonSerializer.DeserializeAsync<List<OrderDto>>(stream, Options, ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

        var result = new List<Order>(dtos.Count);

        for (int i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            result.Add(ConvertOrder(dtos[i], i + 1));
        }

        return result;
    }

    public async IAsyncEnumerable<Order> StreamAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

        int number = 0;

        await foreach (var dto in JsonSerializer.DeserializeAsyncEnumerable<OrderDto>(stream, Options, cancellationToken: ct))
        {
            ct.ThrowIfCancellationRequested();
            number++;

            yield return ConvertOrder(dto, number);
        }
    }

    private static Order ConvertOrder(OrderDto? dto, int number)
    {
        try
        {
            if (dto is null)
                throw new ArgumentException("Порожній запис.");

            if (!Enum.IsDefined(dto.Status))
                throw new ArgumentException("Невідомий статус замовлення.");

            return Order.FromDto(dto);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is DomainException)
        {
            throw new DomainException($"Запис {number}: {ex.Message}", ex);
        }
    }
}