using Microsoft.JSInterop;

namespace DayPulse.Infrastructure.Storage;

/// <summary>
/// Stores string values in IndexedDB through a JavaScript module hosted by the app.
/// </summary>
public sealed class IndexedDbStorage(IJSRuntime jsRuntime) : IIndexedDbStorage, IAsyncDisposable
{
    private const string JavaScriptModulePath = "./js/daypulse-storage.js";

    private readonly IJSRuntime _jsRuntime =
        jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    private readonly SemaphoreSlim _moduleLock = new(1, 1);
    private IJSObjectReference? _module;

    /// <inheritdoc />
    public async ValueTask<string?> GetAsync(
        string databaseName,
        string storeName,
        string key,
        CancellationToken cancellationToken = default)
    {
        ValidateKey(databaseName, storeName, key);

        await _moduleLock.WaitAsync(cancellationToken);
        try
        {
            var module = await GetModuleUnderLockAsync(cancellationToken);
            return await module.InvokeAsync<string?>(
                "indexedDbGet",
                cancellationToken,
                databaseName,
                storeName,
                key);
        }
        finally
        {
            _moduleLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask SetAsync(
        string databaseName,
        string storeName,
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        ValidateKey(databaseName, storeName, key);
        ArgumentNullException.ThrowIfNull(value);

        await _moduleLock.WaitAsync(cancellationToken);
        try
        {
            var module = await GetModuleUnderLockAsync(cancellationToken);
            await module.InvokeVoidAsync(
                "indexedDbSet",
                cancellationToken,
                databaseName,
                storeName,
                key,
                value);
        }
        finally
        {
            _moduleLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask RemoveAsync(
        string databaseName,
        string storeName,
        string key,
        CancellationToken cancellationToken = default)
    {
        ValidateKey(databaseName, storeName, key);

        await _moduleLock.WaitAsync(cancellationToken);
        try
        {
            var module = await GetModuleUnderLockAsync(cancellationToken);
            await module.InvokeVoidAsync(
                "indexedDbRemove",
                cancellationToken,
                databaseName,
                storeName,
                key);
        }
        finally
        {
            _moduleLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _moduleLock.WaitAsync();
        try
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            var module = _module;
            _module = null;

            if (module is not null)
            {
                try
                {
                    await module.DisposeAsync();
                }
                catch (JSDisconnectedException exception)
                {
                    // The browser context has already gone away.
                    System.Diagnostics.Debug.WriteLine(
                        $"IndexedDB storage module disposal was interrupted: {exception.Message}");
                }
            }
        }
        finally
        {
            _moduleLock.Release();
        }
    }

    private async ValueTask<IJSObjectReference> GetModuleUnderLockAsync(
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_module is IJSObjectReference module)
        {
            return module;
        }

        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            cancellationToken,
            JavaScriptModulePath);

        return _module;
    }

    private static void ValidateKey(
        string databaseName,
        string storeName,
        string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(storeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
    }

    private bool _disposed;
}
