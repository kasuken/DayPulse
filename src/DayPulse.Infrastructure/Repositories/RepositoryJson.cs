using System.Text.Json;
using System.Text.Json.Serialization;
using DayPulse.Infrastructure.Storage;

namespace DayPulse.Infrastructure.Repositories;

internal static class RepositoryJson
{
    internal const int MaxMutationAttempts = 8;

    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        };

    internal static async Task<List<T>> ReadListAsync<T>(
        IClientStorage storage,
        string key,
        CancellationToken cancellationToken)
    {
        var snapshot = await ReadListSnapshotAsync<T>(
            storage,
            key,
            cancellationToken);
        return snapshot.Value;
    }

    internal static async Task<(string? Raw, List<T> Value)> ReadListSnapshotAsync<T>(
        IClientStorage storage,
        string key,
        CancellationToken cancellationToken)
    {
        var json = await storage.GetAsync(key, cancellationToken);
        return json is null
            ? (null, [])
            : (json, Deserialize<List<T>>(json, key));
    }

    internal static async Task<T?> ReadAsync<T>(
        IClientStorage storage,
        string key,
        CancellationToken cancellationToken)
        where T : class
    {
        var json = await storage.GetAsync(key, cancellationToken);
        return json is null ? null : Deserialize<T>(json, key);
    }

    internal static ValueTask<bool> TryWriteAsync<T>(
        IAtomicClientStorage storage,
        string key,
        string? expectedRaw,
        T value,
        CancellationToken cancellationToken)
    {
        var json = Serialize(value);
        return storage.TrySetAsync(
            key,
            expectedRaw,
            json,
            cancellationToken);
    }

    internal static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, SerializerOptions);
    }

    internal static T Deserialize<T>(string json, string key)
    {
        try
        {
            var value = JsonSerializer.Deserialize<T>(json, SerializerOptions);
            return value
                ?? throw new InvalidDataException(
                    $"Stored value for '{key}' cannot be null.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Stored value for '{key}' is not valid repository data.",
                exception);
        }
    }
}
