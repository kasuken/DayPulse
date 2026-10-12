using DayPulse.Core.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace DayPulse.App.Shared;

internal sealed class ThemeProviderInterop(
    IJSRuntime jsRuntime,
    ILogger logger) : IAsyncDisposable
{
    internal const string ModulePath = "./Shared/ThemeProvider.razor.js";
    internal const string InitializeMethod = "initialize";
    internal const string SetModeMethod = "setMode";
    internal const string DisposeMethod = "dispose";

    private readonly IJSRuntime _jsRuntime =
        jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    private readonly ILogger _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly object _lifecycleGate = new();
    private readonly SemaphoreSlim _moduleGate = new(1, 1);
    private IJSObjectReference? _module;
    private Task? _disposeTask;
    private bool _disposed;

    internal async ValueTask<bool> InitializeAsync(
        ThemeMode mode,
        DotNetObjectReference<ThemeProvider> dotNetReference,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dotNetReference);

        await _moduleGate.WaitAsync(cancellationToken);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var module = await GetModuleUnderLockAsync(cancellationToken);
            return await module.InvokeAsync<bool>(
                InitializeMethod,
                cancellationToken,
                mode.ToString(),
                dotNetReference);
        }
        finally
        {
            _moduleGate.Release();
        }
    }

    internal async ValueTask<bool> SetModeAsync(
        ThemeMode mode,
        CancellationToken cancellationToken)
    {
        await _moduleGate.WaitAsync(cancellationToken);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var module = await GetModuleUnderLockAsync(cancellationToken);
            return await module.InvokeAsync<bool>(
                SetModeMethod,
                cancellationToken,
                mode.ToString());
        }
        finally
        {
            _moduleGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        Task disposeTask;
        lock (_lifecycleGate)
        {
            _disposeTask ??= DisposeCoreAsync();
            disposeTask = _disposeTask;
        }

        await disposeTask;
    }

    private async Task DisposeCoreAsync()
    {
        _disposed = true;
        await _moduleGate.WaitAsync();
        try
        {
            var module = _module;
            _module = null;
            if (module is null)
            {
                return;
            }

            try
            {
                await module.InvokeVoidAsync(DisposeMethod);
            }
            catch (JSDisconnectedException exception)
            {
                _logger.LogDebug(
                    exception,
                    "DayPulse theme module cleanup ended after the browser disconnected.");
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "DayPulse theme module cleanup failed.");
            }

            try
            {
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException exception)
            {
                // The browser context has already gone away.
                _logger.LogDebug(
                    exception,
                    "DayPulse theme module disposal ended after the browser disconnected.");
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "DayPulse theme module disposal failed.");
            }
        }
        finally
        {
            _moduleGate.Release();
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
            ModulePath);
        return _module;
    }
}
