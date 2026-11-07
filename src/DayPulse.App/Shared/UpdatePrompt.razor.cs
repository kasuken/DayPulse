using Microsoft.JSInterop;
using MudBlazor;

namespace DayPulse.App.Shared;

/// <summary>
/// Offers a reload when a newer version of DayPulse has been downloaded by the service worker.
/// </summary>
/// <remarks>
/// The worker waits instead of taking over, so a running check-in is never replaced underneath
/// the person using it. Choosing Reload activates the new worker and reloads the page.
/// </remarks>
public partial class UpdatePrompt
{
    private const string JavaScriptModulePath = "./Shared/UpdatePrompt.razor.js";

    private readonly CancellationTokenSource _componentCancellation = new();
    private IJSObjectReference? _module;
    private DotNetObjectReference<UpdatePrompt>? _dotNetReference;
    private Task? _initializationTask;
    private bool _shown;
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
    /// Shows the prompt when the service worker reports a waiting update.
    /// </summary>
    [JSInvokable]
    public Task OnUpdateAvailable() => InvokeAsync(ShowSnackbar);

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            _module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                cancellationToken,
                JavaScriptModulePath);
            _dotNetReference = DotNetObjectReference.Create(this);

            var updateWaiting = await _module.InvokeAsync<bool>(
                "initialize",
                cancellationToken,
                _dotNetReference);

            if (updateWaiting)
            {
                await InvokeAsync(ShowSnackbar);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (JSDisconnectedException) when (_disposed)
        {
        }
        catch (JSException)
        {
            // Browsers without service workers simply never offer an update.
        }
    }

    private void ShowSnackbar()
    {
        if (_disposed || _shown)
        {
            return;
        }

        _shown = true;
        Snackbar.Add(
            Localizer["UpdateAvailable"].Value,
            Severity.Info,
            options =>
            {
                options.RequireInteraction = true;
                options.ShowCloseIcon = true;
                options.CloseAfterNavigation = false;
                options.Action = Localizer["UpdateReload"].Value;
                options.ActionColor = Color.Primary;
                options.OnClick = _ => ReloadAsync();
                options.SnackbarVariant = Variant.Filled;
            },
            key: "daypulse-update-available");
    }

    private async Task ReloadAsync()
    {
        if (_disposed || _module is not IJSObjectReference module)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync("applyUpdate", _componentCancellation.Token);
        }
        catch (OperationCanceledException) when (_componentCancellation.IsCancellationRequested)
        {
        }
        catch (JSDisconnectedException) when (_disposed)
        {
        }
    }

    /// <inheritdoc />
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
