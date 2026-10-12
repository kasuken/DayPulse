using Microsoft.JSInterop;

namespace DayPulse.Infrastructure.Storage;

/// <summary>
/// Stores string values in the browser's local storage through JavaScript interop.
/// </summary>
public sealed class BrowserStorage(IJSRuntime jsRuntime) :
    IAtomicClientStorage,
    IAsyncDisposable
{
    private const string JavaScriptModulePath = "./js/daypulse-storage.js";

    private readonly IJSRuntime _jsRuntime =
        jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    private readonly SemaphoreSlim _moduleLock = new(1, 1);
    private IJSObjectReference? _module;

    /// <inheritdoc />
    public ValueTask<string?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return _jsRuntime.InvokeAsync<string?>(
            "localStorage.getItem",
            cancellationToken,
            key);
    }

    /// <inheritdoc />
    public ValueTask SetAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        return _jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            cancellationToken,
            key,
            value);
    }

    /// <inheritdoc />
    public ValueTask RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return _jsRuntime.InvokeVoidAsync(
            "localStorage.removeItem",
            cancellationToken,
            key);
    }

    /// <inheritdoc />
    public async ValueTask<bool> TrySetAsync(
        string key,
        string? expectedValue,
        string value,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        await _moduleLock.WaitAsync(cancellationToken);
        try
        {
            var module = await GetModuleUnderLockAsync(cancellationToken);
            return await module.InvokeAsync<bool>(
                "localStorageTrySet",
                cancellationToken,
                key,
                expectedValue,
                value);
        }
        finally
        {
            _moduleLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryRemoveAsync(
        string key,
        string? expectedValue,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await _moduleLock.WaitAsync(cancellationToken);
        try
        {
            var module = await GetModuleUnderLockAsync(cancellationToken);
            return await module.InvokeAsync<bool>(
                "localStorageTryRemove",
                cancellationToken,
                key,
                expectedValue);
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
                        $"Browser storage module disposal was interrupted: {exception.Message}");
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

    private bool _disposed;
}
