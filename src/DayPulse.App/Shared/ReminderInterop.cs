using Microsoft.JSInterop;

namespace DayPulse.App.Shared;

/// <summary>Provides optional browser notifications without sending user data to a server.</summary>
public sealed class ReminderInterop(IJSRuntime js) : IAsyncDisposable
{
    internal const string ModulePath = "./Shared/DailyReminderHost.razor.js";
    private IJSObjectReference? _module;

    private async ValueTask<IJSObjectReference> ModuleAsync()
        => _module ??= await js.InvokeAsync<IJSObjectReference>("import", ModulePath);

    /// <summary>Requests notification permission following a user action.</summary>
    public async ValueTask<string> RequestPermissionAsync()
        => await (await ModuleAsync()).InvokeAsync<string>("requestPermission");

    /// <summary>Claims today's reminder and optionally shows a generic browser notification.</summary>
    public async ValueTask<bool> ShowOnceAsync(string date, string title, string message)
        => await (await ModuleAsync()).InvokeAsync<bool>("showOnce", date, title, message);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
