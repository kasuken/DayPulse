using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DayPulse.App.Shared;

/// <summary>
/// Displays mobile installation guidance when DayPulse is not running as an installed PWA.
/// </summary>
public partial class InstallPrompt : IAsyncDisposable
{
    private const string JavaScriptModulePath = "./Shared/InstallPrompt.razor.js";

    private readonly CancellationTokenSource _componentCancellation = new();
    private IJSObjectReference? _module;
    private DotNetObjectReference<InstallPrompt>? _dotNetReference;
    private Task? _initializationTask;
    private InstallPromptState _state;
    private bool _beforeInstallPromptAvailable;
    private bool _hasState;
    private bool _isVisible;
    private bool _sessionDismissed;
    private bool _disposed;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _initializationTask = InitializeAsync(_componentCancellation.Token);
        await _initializationTask;
    }

    /// <summary>
    /// Refreshes the prompt when the browser exposes its native installation prompt.
    /// </summary>
    [JSInvokable]
    public async Task OnBeforeInstallPromptAvailable()
    {
        _beforeInstallPromptAvailable = true;

        if (_disposed || !_hasState || _sessionDismissed)
        {
            return;
        }

        await InvokeAsync(() =>
        {
            if (_disposed || !_hasState || _sessionDismissed)
            {
                return;
            }

            _state = _state with { CanPrompt = true };
            _isVisible = ShouldShow(_state);
            StateHasChanged();
        });
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            _module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                cancellationToken,
                JavaScriptModulePath);
            _dotNetReference = DotNetObjectReference.Create(this);

            _state = await _module.InvokeAsync<InstallPromptState>(
                "initialize",
                cancellationToken,
                _dotNetReference);
            _state = _state with
            {
                CanPrompt = _state.CanPrompt || _beforeInstallPromptAvailable
            };
            _hasState = true;
            _isVisible = ShouldShow(_state);

            if (_isVisible && !_disposed)
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (JSDisconnectedException) when (_disposed)
        {
        }
    }

    private async Task InstallAsync()
    {
        if (_disposed || _module is not IJSObjectReference module)
        {
            return;
        }

        try
        {
            var result = await module.InvokeAsync<InstallPromptResult>(
                "promptInstall",
                _componentCancellation.Token);
            if (result.WasAvailable)
            {
                _isVisible = false;
            }
            else
            {
                _state = _state with { CanPrompt = false };
                _isVisible = ShouldShow(_state);
            }

            if (!_disposed)
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException) when (_componentCancellation.IsCancellationRequested)
        {
        }
        catch (JSDisconnectedException) when (_disposed)
        {
        }
    }

    private async Task DismissAsync()
    {
        if (_disposed)
        {
            return;
        }

        _sessionDismissed = true;
        _isVisible = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task DismissPermanentlyAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            if (_module is IJSObjectReference module)
            {
                await module.InvokeVoidAsync(
                    "dismiss",
                    _componentCancellation.Token,
                    true);
                _state = _state with { IsDismissed = true };
            }
        }
        catch (OperationCanceledException) when (_componentCancellation.IsCancellationRequested)
        {
            return;
        }
        catch (JSDisconnectedException) when (_disposed)
        {
            return;
        }

        _sessionDismissed = true;
        _isVisible = false;
        await InvokeAsync(StateHasChanged);
    }

    private static bool ShouldShow(InstallPromptState state)
    {
        return state.IsMobile
            && (state.IsIosSafari || state.IsAndroid)
            && !state.IsStandalone
            && !state.IsDismissed;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _componentCancellation.Cancel();

        if (_initializationTask is not null)
        {
            await _initializationTask;
        }

        var module = _module;
        _module = null;

        if (module is not null)
        {
            try
            {
                await module.InvokeVoidAsync("dispose");
            }
            catch (JSDisconnectedException)
            {
                // The browser context has already gone away.
            }

            try
            {
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The browser context has already gone away.
            }
        }

        _dotNetReference?.Dispose();
        _componentCancellation.Dispose();
    }
}

/// <summary>
/// Describes the browser state returned by the install prompt JavaScript module.
/// </summary>
public readonly record struct InstallPromptState(
    [property: JsonPropertyName("isStandalone")] bool IsStandalone,
    [property: JsonPropertyName("isIosSafari")] bool IsIosSafari,
    [property: JsonPropertyName("isAndroid")] bool IsAndroid,
    [property: JsonPropertyName("isMobile")] bool IsMobile,
    [property: JsonPropertyName("isDismissed")] bool IsDismissed,
    [property: JsonPropertyName("canPrompt")] bool CanPrompt);

/// <summary>
/// Describes the outcome of a browser-native installation prompt.
/// </summary>
public readonly record struct InstallPromptResult(
    [property: JsonPropertyName("wasAvailable")] bool WasAvailable,
    [property: JsonPropertyName("wasAccepted")] bool WasAccepted);
