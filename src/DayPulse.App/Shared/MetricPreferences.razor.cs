using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using Microsoft.AspNetCore.Components;

namespace DayPulse.App.Shared;

/// <summary>Edits optional metric visibility without changing unrelated preferences.</summary>
public partial class MetricPreferences
{
    [Inject] private ISettingsRepository Repository { get; set; } = default!;
    [Inject] private ILogger<MetricPreferences> Logger { get; set; } = default!;
    private AppSettings? _settings;
    private bool _busy;
    private bool _error;
    private bool _disposed;
    private int _revision;
    private readonly CancellationTokenSource _lifetime = new();
    private Task _pending = Task.CompletedTask;

    protected override Task OnInitializedAsync() => LoadAsync();

    private Task LoadAsync() => _pending = LoadCoreAsync();

    private async Task LoadCoreAsync()
    {
        _busy = true;
        _error = false;
        try
        {
            _settings = await Repository.LoadAsync(_lifetime.Token);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Logger.LogDebug("Metric preference load canceled during disposal.");
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Metric preferences could not be loaded.");
            _error = true;
        }
        finally
        {
            _busy = false;
        }
    }

    private Task SaveAsync(Metric metric, bool enabled)
    {
        if (_busy || _disposed)
        {
            return Task.CompletedTask;
        }

        return _pending = SaveCoreAsync(metric, enabled);
    }

    private async Task SaveCoreAsync(Metric metric, bool enabled)
    {
        _busy = true;
        _error = false;
        try
        {
            _settings = await Repository.UpdateAsync(current => new AppSettings(
                current.OnboardingCompleted, current.ThemeMode,
                current.ReminderEnabled, current.ReminderTime,
                metric == Metric.Energy ? enabled : current.EnergyEnabled,
                metric == Metric.Anxiety ? enabled : current.AnxietyEnabled,
                metric == Metric.Focus ? enabled : current.FocusEnabled,
                metric == Metric.Sleep ? enabled : current.SleepEnabled), _lifetime.Token);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Logger.LogDebug("Metric preference save canceled during disposal.");
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Metric preference could not be saved.");
            _error = true;
            _revision++;
        }
        finally
        {
            _busy = false;
        }
    }

    private enum Metric { Energy, Anxiety, Focus, Sleep }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        await _lifetime.CancelAsync();
        await _pending;
        _lifetime.Dispose();
    }
}
