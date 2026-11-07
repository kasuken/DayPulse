using Microsoft.JSInterop;

namespace DayPulse.Infrastructure.Storage;

/// <summary>
/// Stores string values in the browser's IndexedDB through a JavaScript module hosted by the app.
/// </summary>
/// <remarks>
/// Each operation, including the conditional ones, runs in a single IndexedDB transaction. Values
/// that earlier versions kept in localStorage are copied into IndexedDB the first time the module
/// is used, and the legacy copies are then removed.
/// </remarks>
public sealed class IndexedDbClientStorage(IJSRuntime jsRuntime) :
    IAtomicClientStorage,
    IAsyncDisposable
{
    private const string JavaScriptModulePath = "./js/daypulse-storage.js";

    private readonly IJSRuntime _jsRuntime =
        jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    private readonly SemaphoreSlim _moduleLock = new(1, 1);
    private IJSObjectReference? _module;
    private bool _disposed;

    /// <inheritdoc />
    public async ValueTask<string?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var module = await GetModuleAsync(cancellationToken);
        return await module.InvokeAsync<string?>("storageGet", cancellationToken, key);
    }

    /// <inheritdoc />
    public async ValueTask SetAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        var module = await GetModuleAsync(cancellationToken);
        await module.InvokeVoidAsync("storageSet", cancellationToken, key, value);
    }

    /// <inheritdoc />
    public async ValueTask RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var module = await GetModuleAsync(cancellationToken);
        await module.InvokeVoidAsync("storageRemove", cancellationToken, key);
    }

    /// <inheritdoc />
    public async ValueTask<bool> TrySetAsync(
        string key,
        string? expectedValue,
        string value,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        var module = await GetModuleAsync(cancellationToken);
        return await module.InvokeAsync<bool>(
            "storageTrySet",
            cancellationToken,
            key,
            expectedValue,
            value);
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryRemoveAsync(
        string key,
        string? expectedValue,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var module = await GetModuleAsync(cancellationToken);
        return await module.InvokeAsync<bool>(
            "storageTryRemove",
            cancellationToken,
            key,
            expectedValue);
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

    private async ValueTask<IJSObjectReference> GetModuleAsync(
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _moduleLock.WaitAsync(cancellationToken);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                cancellationToken,
                JavaScriptModulePath);

            return _module;
        }
        finally
        {
            _moduleLock.Release();
        }
    }
}
