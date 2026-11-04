using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using Microsoft.AspNetCore.Components;

namespace DayPulse.App.Shared;

/// <summary>
/// Explains local-only storage and records the initial metric preferences.
/// </summary>
public partial class Onboarding : IAsyncDisposable
{
    [Inject] private ISettingsRepository SettingsRepository { get; set; } = default!;
    [Inject] private ILogger<Onboarding> Logger { get; set; } = default!;

    /// <summary>Gets or sets the preferences loaded by the application.</summary>
    [Parameter, EditorRequired] public AppSettings InitialSettings { get; set; } = new();

    /// <summary>Gets or sets the event raised after preferences are persisted.</summary>
    [Parameter] public EventCallback<AppSettings> OnCompleted { get; set; }

    private readonly CancellationTokenSource _cancellation = new();
    private Task? _saveTask;
    private int _step;
    private bool _energy;
    private bool _anxiety;
    private bool _focus;
    private bool _sleep;
    private bool _saving;
    private bool _failed;
    private bool _disposed;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _energy = InitialSettings.EnergyEnabled;
        _anxiety = InitialSettings.AnxietyEnabled;
        _focus = InitialSettings.FocusEnabled;
        _sleep = InitialSettings.SleepEnabled;
    }

    private void Next() => _step = Math.Min(2, _step + 1);
    private void Back() => _step = Math.Max(0, _step - 1);

    private Task FinishAsync()
    {
        if (_saving || _disposed)
        {
            return Task.CompletedTask;
        }

        _saving = true;
        _saveTask = SaveAsync();
        return _saveTask;
    }

    private async Task SaveAsync()
    {
        _failed = false;
        try
        {
            var saved = await SettingsRepository.UpdateAsync(current => new AppSettings(
                onboardingCompleted: true,
                themeMode: current.ThemeMode,
                reminderEnabled: current.ReminderEnabled,
                reminderTime: current.ReminderTime,
                energyEnabled: _energy,
                anxietyEnabled: _anxiety,
                focusEnabled: _focus,
                sleepEnabled: _sleep), _cancellation.Token);

            if (!_disposed)
            {
                await OnCompleted.InvokeAsync(saved);
            }
        }
        catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
        {
            Logger.LogDebug("Onboarding save was canceled during disposal.");
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Initial preferences could not be saved.");
            _failed = true;
        }
        finally
        {
            _saving = false;
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
        _cancellation.Cancel();
        // The completion callback can remove this component during SaveAsync.
        // Cancellation is enough; awaiting that same callback would deadlock disposal.
        if (_saveTask is { IsCompleted: true })
        {
            await _saveTask;
        }
        _cancellation.Dispose();
    }
}
