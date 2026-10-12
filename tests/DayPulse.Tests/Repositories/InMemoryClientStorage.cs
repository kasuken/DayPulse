using DayPulse.Infrastructure.Storage;

namespace DayPulse.Tests.Repositories;

internal sealed class InMemoryClientStorage : IAtomicClientStorage
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);
    private readonly object _sync = new();

    public int SetCount { get; private set; }

    public int ConditionalWriteFailuresRemaining { get; set; }

    public string? InjectedConditionalWriteKey { get; set; }

    public string? InjectedConditionalWriteValue { get; set; }

    public ValueTask<string?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return ValueTask.FromResult(
                _values.TryGetValue(key, out var value) ? value : null);
        }
    }

    public ValueTask SetAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            _values[key] = value;
            SetCount++;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            _values.Remove(key);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> TrySetAsync(
        string key,
        string? expectedValue,
        string value,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            if (string.Equals(key, InjectedConditionalWriteKey, StringComparison.Ordinal))
            {
                _values[key] = InjectedConditionalWriteValue
                    ?? throw new InvalidOperationException(
                        "An injected conditional-write value is required.");
                InjectedConditionalWriteKey = null;
                InjectedConditionalWriteValue = null;
            }

            if (ConditionalWriteFailuresRemaining > 0)
            {
                ConditionalWriteFailuresRemaining--;
                return ValueTask.FromResult(false);
            }

            var currentValue = _values.TryGetValue(key, out var storedValue)
                ? storedValue
                : null;
            if (!string.Equals(currentValue, expectedValue, StringComparison.Ordinal))
            {
                return ValueTask.FromResult(false);
            }

            _values[key] = value;
            SetCount++;
            return ValueTask.FromResult(true);
        }
    }

    public ValueTask<bool> TryRemoveAsync(
        string key,
        string? expectedValue,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var currentValue = _values.TryGetValue(key, out var storedValue)
                ? storedValue
                : null;
            if (!string.Equals(currentValue, expectedValue, StringComparison.Ordinal))
            {
                return ValueTask.FromResult(false);
            }

            _values.Remove(key);
            return ValueTask.FromResult(true);
        }
    }
}
