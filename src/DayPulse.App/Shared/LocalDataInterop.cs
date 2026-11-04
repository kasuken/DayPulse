using Microsoft.JSInterop;

namespace DayPulse.App.Shared;

/// <summary>Downloads local exports and clears only DayPulse's install dismissal preference.</summary>
public sealed class LocalDataInterop(IJSRuntime runtime) : IAsyncDisposable
{
    private IJSObjectReference? _module;

    /// <summary>Starts a download without uploading user data.</summary>
    public async Task DownloadAsync(string filename, string contentType, string content)
    {
        var module = await ModuleAsync();
        await module.InvokeVoidAsync("download", filename, contentType, content);
    }

    /// <summary>Removes the install prompt dismissal without clearing unrelated site data.</summary>
    public async Task ResetInstallDismissalAsync()
    {
        var module = await ModuleAsync();
        await module.InvokeVoidAsync("resetInstallDismissal");
    }

    private async Task<IJSObjectReference> ModuleAsync() =>
        _module ??= await runtime.InvokeAsync<IJSObjectReference>("import", "./Shared/DataControls.razor.js");

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
