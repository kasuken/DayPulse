using DayPulse.Core.Domain;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace DayPulse.App.Pages;

public partial class EntryDetail
{
    /// <summary>Gets or sets the check-in identifier.</summary>
    [Parameter] public string Id { get; set; } = string.Empty;

    private readonly CancellationTokenSource _lifetime = new();
    private CheckIn? _entry;
    private IReadOnlyList<Activity> _activities = [];
    private bool _loading = true;
    private bool _loadFailed;
    private bool _confirmDelete;
    private bool _deleteFailed;
    private bool _deleting;
    private int _loadVersion;

    protected override Task OnParametersSetAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        var version = ++_loadVersion;
        var id = Id;
        _loading = true;
        _loadFailed = false;
        _confirmDelete = false;
        _deleteFailed = false;
        try
        {
            var entry = await Entries.GetByIdAsync(id, _lifetime.Token);
            var activities = await Activities.GetAllAsync(_lifetime.Token);
            if (version != _loadVersion) return;
            _entry = entry;
            _activities = activities;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Unable to load a journal entry.");
            if (version == _loadVersion) _loadFailed = true;
        }
        finally { if (version == _loadVersion) _loading = false; }
    }

    private async Task DeleteAsync()
    {
        if (_entry is null || _deleting || !_confirmDelete) return;
        _deleting = true;
        _deleteFailed = false;
        try
        {
            await Entries.DeleteAsync(_entry.Id, _lifetime.Token);
            Navigation.NavigateTo("/history");
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Unable to delete a journal entry.");
            _deleteFailed = true;
        }
        finally { _deleting = false; }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _lifetime.CancelAsync();
        _lifetime.Dispose();
    }
}
