using DayPulse.Core.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DayPulse.App.Shared;

/// <summary>Exports local data and explicitly confirms irreversible data removal.</summary>
public partial class DataControls
{
    [Inject] private ICheckInRepository CheckIns { get; set; } = default!;
    [Inject] private IActivityRepository Activities { get; set; } = default!;
    [Inject] private ISettingsRepository Settings { get; set; } = default!;
    [Inject] private IJSRuntime Runtime { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<DataControls> Logger { get; set; } = default!;
    private LocalDataInterop? _interop;
    private Task _pending = Task.CompletedTask;
    private bool _busy;
    private bool _confirming;
    private bool _confirmed;
    private bool _downloadStarted;
    private bool _disposed;
    private string? _errorKey;

    private void RequestDelete()
    {
        _confirming = true;
        _confirmed = false;
        _downloadStarted = false;
    }

    private void CancelDelete()
    {
        _confirming = false;
        _confirmed = false;
    }

    private Task ExportAsync(bool csv)
    {
        if (_busy || _disposed)
        {
            return Task.CompletedTask;
        }

        _pending = ExportCoreAsync(csv);
        return _pending;
    }

    private async Task ExportCoreAsync(bool csv)
    {
        _busy = true;
        _errorKey = null;
        _downloadStarted = false;
        try
        {
            var checkIns = await CheckIns.GetAllAsync();
            var activities = await Activities.GetAllAsync();
            var settings = await Settings.LoadAsync();
            var content = csv
                ? SettingsDataExport.Csv(checkIns, activities, settings)
                : SettingsDataExport.Json(checkIns, activities, settings);
            _interop ??= new LocalDataInterop(Runtime);
            await _interop.DownloadAsync(csv ? "daypulse.csv" : "daypulse.json",
                csv ? "text/csv;charset=utf-8" : "application/json;charset=utf-8", content);
            _downloadStarted = true;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Local data export failed.");
            _errorKey = "DataControlsExportError";
        }
        finally
        {
            _busy = false;
        }
    }

    private Task DeleteAsync()
    {
        if (!_confirmed || !_confirming || _busy || _disposed)
        {
            return Task.CompletedTask;
        }

        _pending = DeleteCoreAsync();
        return _pending;
    }

    private async Task DeleteCoreAsync()
    {
        _busy = true;
        _errorKey = null;
        _downloadStarted = false;
        var failed = false;
        try
        {
            // These stores cannot share a transaction. Attempt independent removals,
            // retain an honest partial-failure state, and never report a false success.
            failed |= !await TryDeleteStepAsync(() => CheckIns.DeleteAllAsync());
            try
            {
                var activities = await Activities.GetAllAsync();
                foreach (var activity in activities.Where(activity => !activity.IsDefault))
                {
                    failed |= !await TryDeleteStepAsync(() => Activities.DeleteAsync(activity.Id));
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "The custom activity catalog could not be read for deletion.");
                failed = true;
            }

            if (!failed)
            {
                _interop ??= new LocalDataInterop(Runtime);
                failed |= !await TryDeleteStepAsync(() => _interop.ResetInstallDismissalAsync());
            }

            if (!failed)
            {
                failed |= !await TryDeleteStepAsync(() => Settings.ResetAsync());
            }

            if (failed)
            {
                _errorKey = "DataControlsDeleteError";
            }
            else if (!_disposed)
            {
                // Reload the app so both onboarding and the theme read reset settings.
                Navigation.NavigateTo(Navigation.BaseUri, forceLoad: true);
            }
        }
        finally
        {
            _busy = false;
            _confirmed = false;
        }
    }

    private async Task<bool> TryDeleteStepAsync(Func<Task> operation)
    {
        try
        {
            await operation();
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "A local data removal step failed.");
            return false;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        await _pending;
        if (_interop is not null)
        {
            try
            {
                await _interop.DisposeAsync();
            }
            catch (JSException exception)
            {
                Logger.LogDebug(exception, "Local data module was unavailable during disposal.");
            }
        }
    }
}
