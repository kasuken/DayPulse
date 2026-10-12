using DayPulse.Core.Domain;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace DayPulse.App.Shared;

/// <summary>
/// Applies the selected DayPulse theme and tracks the browser system preference.
/// </summary>
public partial class ThemeProvider
{
    private readonly CancellationTokenSource _componentCancellation = new();
    private readonly object _lifecycleGate = new();
    private readonly HashSet<Task> _pendingTasks = [];
    private readonly MudTheme _theme = DayPulseTheme.Create();
    private ThemeProviderInterop? _interop;
    private DotNetObjectReference<ThemeProvider>? _dotNetReference;
    private Task? _stateRefreshTask;
    private Task? _initializationTask;
    private Task? _modeApplicationTask;
    private Task? _disposeTask;
    private bool _modeApplicationPending;
    private bool _refreshQueued;
    private bool _retryRequested;
    private bool _initialized;
    private bool _disposed;

    protected override void OnInitialized()
    {
        ThemeState.RegisterProvider();
        ThemeState.Changed += OnThemeStateChanged;
        ThemeState.RetryRequested += OnThemeRetryRequested;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_disposed)
        {
            return;
        }

        if (firstRender)
        {
            _initializationTask = TrackPendingTaskAsync(
                () => InitializeAsync(_componentCancellation.Token));
            await _initializationTask;
            return;
        }

        if (_modeApplicationPending && _interop is not null)
        {
            _modeApplicationTask = TrackPendingTaskAsync(
                () => ApplyPendingModeAsync(_componentCancellation.Token));
            await _modeApplicationTask;
        }
    }

    /// <summary>
    /// Applies a browser system theme change without changing the persisted preference.
    /// </summary>
    /// <param name="isDarkMode">Whether the browser now prefers dark mode.</param>
    [JSInvokable]
    public Task OnSystemThemeChanged(bool isDarkMode)
    {
        if (_disposed || !_initialized || ThemeState.Mode != ThemeMode.System)
        {
            return Task.CompletedTask;
        }

        return TrackPendingTaskAsync(
            () => OnSystemThemeChangedAsync(isDarkMode));
    }

    private async Task OnSystemThemeChangedAsync(bool isDarkMode)
    {
        try
        {
            if (!ThemeState.ApplySystemPreference(isDarkMode))
            {
                return;
            }

            await InvokeAsync(() =>
            {
                if (!_disposed)
                {
                    StateHasChanged();
                }
            });
        }
        catch (OperationCanceledException)
            when (_componentCancellation.IsCancellationRequested)
        {
            Logger.LogDebug(
                "DayPulse system theme update was canceled during component disposal.");
        }
        catch (JSDisconnectedException exception)
        {
            Logger.LogDebug(
                exception,
                "DayPulse system theme update ended after the browser disconnected.");
        }
        catch (ObjectDisposedException exception) when (_disposed)
        {
            Logger.LogDebug(
                exception,
                "DayPulse system theme update ended during component disposal.");
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "DayPulse system theme update failed.");
        }
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        ThemeProviderInterop? initializationInterop = null;
        DotNetObjectReference<ThemeProvider>? initializationReference = null;
        var providerInitializationFailed = false;

        try
        {
            await ThemeState.InitializeAsync(cancellationToken);
            if (_disposed)
            {
                return;
            }

            await DisposeCurrentInteropAsync();
            if (_disposed)
            {
                return;
            }

            initializationInterop = new ThemeProviderInterop(JSRuntime, Logger);
            initializationReference = DotNetObjectReference.Create(this);

            var mode = ThemeState.Mode;
            var isDarkMode = await initializationInterop.InitializeAsync(
                mode,
                initializationReference,
                cancellationToken);

            var modeApplicationPending = ThemeState.Mode != mode;
            if (!modeApplicationPending)
            {
                ThemeState.ApplySystemPreference(isDarkMode);
            }
            _modeApplicationPending = modeApplicationPending;

            lock (_lifecycleGate)
            {
                if (_disposed)
                {
                    return;
                }

                _interop = initializationInterop;
                _dotNetReference = initializationReference;
                initializationInterop = null;
                initializationReference = null;
                _initialized = true;
                _retryRequested = false;
            }

            ThemeState.MarkProviderInitialized();
            _modeApplicationPending = modeApplicationPending;
            await InvokeAsync(StateHasChanged);
            ThemeState.CompleteProviderRetry(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogDebug(
                "DayPulse theme initialization was canceled during component disposal.");
        }
        catch (JSDisconnectedException exception)
        {
            providerInitializationFailed = !_disposed && ThemeState.IsInitialized;
            Logger.LogDebug(
                exception,
                "DayPulse theme initialization ended after the browser disconnected.");
        }
        catch (ObjectDisposedException) when (_disposed)
        {
            Logger.LogDebug(
                "DayPulse theme initialization ended during component disposal.");
        }
        catch (Exception exception)
        {
            _initialized = false;
            providerInitializationFailed = !_disposed && ThemeState.IsInitialized;
            Logger.LogError(exception, "DayPulse theme initialization failed.");
        }
        finally
        {
            await DisposeInteropResourcesAsync(
                initializationInterop,
                initializationReference);
        }

        if (!providerInitializationFailed)
        {
            return;
        }

        await ReportProviderInitializationFailureAsync();
    }

    private async Task ApplyPendingModeAsync(CancellationToken cancellationToken)
    {
        var interop = _interop;
        if (interop is null || _disposed)
        {
            return;
        }

        var mode = ThemeState.Mode;
        _modeApplicationPending = false;

        try
        {
            var isDarkMode = await interop.SetModeAsync(mode, cancellationToken);
            if (ThemeState.Mode != mode)
            {
                _modeApplicationPending = true;
                return;
            }

            if (ThemeState.ApplySystemPreference(isDarkMode))
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogDebug(
                "DayPulse theme mode update was canceled during component disposal.");
        }
        catch (JSDisconnectedException exception)
        {
            _modeApplicationPending = true;
            await ReportProviderInitializationFailureAsync();
            Logger.LogDebug(
                exception,
                "DayPulse theme mode update ended after the browser disconnected.");
        }
        catch (ObjectDisposedException) when (_disposed)
        {
            Logger.LogDebug(
                "DayPulse theme mode update ended during component disposal.");
        }
        catch (Exception exception)
        {
            _modeApplicationPending = true;
            await ReportProviderInitializationFailureAsync();
            Logger.LogError(exception, "DayPulse theme mode update failed.");
        }
    }

    private void OnThemeStateChanged(object? sender, EventArgs args)
    {
        lock (_lifecycleGate)
        {
            if (_disposed)
            {
                return;
            }

            _modeApplicationPending = true;
            if (!_initialized
                && ThemeState.IsInitialized
                && _initializationTask is { IsCompleted: true })
            {
                _initializationTask = TrackPendingTaskAsync(
                    () => InitializeAsync(_componentCancellation.Token));
            }

            if (_refreshQueued)
            {
                return;
            }

            _refreshQueued = true;
        }

        _stateRefreshTask = TrackPendingTaskAsync(
            RefreshAfterThemeStateChangedAsync);
    }

    private void OnThemeRetryRequested(object? sender, EventArgs args)
    {
        lock (_lifecycleGate)
        {
            if (_disposed || _initialized || !ThemeState.IsInitialized)
            {
                return;
            }

            if (_initializationTask is { IsCompleted: false })
            {
                _retryRequested = true;
                return;
            }

            _initializationTask = TrackPendingTaskAsync(
                () => InitializeAsync(_componentCancellation.Token));
        }
    }

    private async Task RefreshAfterThemeStateChangedAsync()
    {
        try
        {
            if (_disposed)
            {
                return;
            }

            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
            when (_componentCancellation.IsCancellationRequested)
        {
            Logger.LogDebug(
                "DayPulse theme state refresh was canceled during component disposal.");
        }
        catch (JSDisconnectedException exception)
        {
            Logger.LogDebug(
                exception,
                "DayPulse theme state refresh ended after the browser disconnected.");
        }
        catch (ObjectDisposedException exception) when (_disposed)
        {
            Logger.LogDebug(
                exception,
                "DayPulse theme state refresh ended during component disposal.");
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "DayPulse theme state refresh failed.");
        }
        finally
        {
            lock (_lifecycleGate)
            {
                _refreshQueued = false;
            }
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        lock (_lifecycleGate)
        {
            _disposeTask ??= DisposeCoreAsync();
            return new ValueTask(_disposeTask);
        }
    }

    private async Task DisposeCoreAsync()
    {
        Task[] pendingTasks;
        lock (_lifecycleGate)
        {
            _disposed = true;
            ThemeState.Changed -= OnThemeStateChanged;
            ThemeState.RetryRequested -= OnThemeRetryRequested;
            _componentCancellation.Cancel();
            pendingTasks = [.. _pendingTasks];
        }

        try
        {
            await Task.WhenAll(pendingTasks);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "DayPulse theme component work did not stop cleanly.");
        }

        ThemeProviderInterop? interop;
        DotNetObjectReference<ThemeProvider>? dotNetReference;
        lock (_lifecycleGate)
        {
            interop = _interop;
            dotNetReference = _dotNetReference;
            _interop = null;
            _dotNetReference = null;
        }
        await DisposeInteropResourcesAsync(interop, dotNetReference);
        ThemeState.UnregisterProvider();
        _componentCancellation.Dispose();
    }

    private async Task ReportProviderInitializationFailureAsync()
    {
        _initialized = false;
        _modeApplicationPending = true;
        await DisposeCurrentInteropAsync();

        if (_disposed || !ThemeState.IsInitialized)
        {
            ThemeState.CompleteProviderRetry(false);
            return;
        }

        try
        {
            ThemeState.MarkProviderInitializationFailed();
        }
        catch (ObjectDisposedException) when (_disposed)
        {
            Logger.LogDebug(
                "DayPulse theme provider failure state ended during disposal.");
        }
        catch (Exception stateException)
        {
            Logger.LogError(
                stateException,
                "DayPulse theme provider failure state could not be recorded.");
        }

        if (!StartQueuedInitializationIfNeeded())
        {
            ThemeState.CompleteProviderRetry(false);
        }
    }

    private bool StartQueuedInitializationIfNeeded()
    {
        lock (_lifecycleGate)
        {
            if (!_retryRequested
                || _disposed
                || _initialized
                || !ThemeState.IsInitialized)
            {
                return false;
            }

            _retryRequested = false;
            _initializationTask = TrackPendingTaskAsync(
                () => InitializeAsync(_componentCancellation.Token));
            return true;
        }
    }

    private async Task DisposeCurrentInteropAsync()
    {
        ThemeProviderInterop? interop;
        DotNetObjectReference<ThemeProvider>? dotNetReference;
        lock (_lifecycleGate)
        {
            interop = _interop;
            dotNetReference = _dotNetReference;
            _interop = null;
            _dotNetReference = null;
        }

        await DisposeInteropResourcesAsync(interop, dotNetReference);
    }

    private async Task DisposeInteropResourcesAsync(
        ThemeProviderInterop? interop,
        DotNetObjectReference<ThemeProvider>? dotNetReference)
    {
        if (interop is not null)
        {
            try
            {
                await interop.DisposeAsync();
            }
            catch (JSDisconnectedException exception)
            {
                Logger.LogDebug(
                    exception,
                    "DayPulse theme interop disposal ended after the browser disconnected.");
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "DayPulse theme interop disposal failed.");
            }
        }

        if (dotNetReference is not null)
        {
            try
            {
                dotNetReference.Dispose();
            }
            catch (Exception exception)
            {
                Logger.LogError(
                    exception,
                    "DayPulse theme .NET callback reference disposal failed.");
            }
        }
    }

    private async Task TrackPendingTaskAsync(Func<Task> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var completion = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_lifecycleGate)
        {
            if (_disposed)
            {
                return;
            }

            _pendingTasks.Add(completion.Task);
        }

        try
        {
            await operation();
            completion.TrySetResult(null);
        }
        catch (Exception exception)
        {
            completion.TrySetException(exception);
            throw;
        }
        finally
        {
            lock (_lifecycleGate)
            {
                _pendingTasks.Remove(completion.Task);
            }
        }
    }
}
