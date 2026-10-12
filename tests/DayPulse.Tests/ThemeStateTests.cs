using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;

namespace DayPulse.Tests;

public sealed class ThemeStateTests
{
    [Fact]
    public async Task InitializeAsync_restores_the_persisted_theme_mode()
    {
        var repository = new InMemorySettingsRepository(
            new AppSettings(themeMode: ThemeMode.Dark));
        var state = new ThemeState(repository);

        await state.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.True(state.IsInitialized);
        Assert.Equal(ThemeMode.Dark, state.Mode);
        Assert.True(state.IsDarkMode);
    }

    [Fact]
    public async Task InitializeAsync_exposes_failure_and_allows_a_retry()
    {
        var repository = new FailingOnceSettingsRepository(
            new AppSettings(themeMode: ThemeMode.Dark));
        var state = new ThemeState(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            state.InitializeAsync(TestContext.Current.CancellationToken));

        Assert.False(state.IsInitialized);
        Assert.True(state.IsInitializationFailed);

        await state.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.True(state.IsInitialized);
        Assert.False(state.IsInitializationFailed);
        Assert.Equal(ThemeMode.Dark, state.Mode);
    }

    [Fact]
    public async Task RetryInitializationAsync_notifies_provider_when_settings_are_loaded()
    {
        var repository = new InMemorySettingsRepository(
            new AppSettings(themeMode: ThemeMode.System));
        var state = new ThemeState(repository);
        var changeCount = 0;
        state.Changed += (_, _) => changeCount++;

        await state.InitializeAsync(TestContext.Current.CancellationToken);
        var initialChangeCount = changeCount;

        await state.RetryInitializationAsync(TestContext.Current.CancellationToken);

        Assert.Equal(initialChangeCount + 1, changeCount);
    }

    [Fact]
    public async Task System_mode_follows_browser_preference_changes()
    {
        var repository = new InMemorySettingsRepository(
            new AppSettings(themeMode: ThemeMode.System));
        var state = new ThemeState(repository);

        await state.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.False(state.IsDarkMode);
        Assert.True(state.ApplySystemPreference(true));
        Assert.True(state.IsDarkMode);
        Assert.True(state.ApplySystemPreference(false));
        Assert.False(state.IsDarkMode);
    }

    [Fact]
    public async Task Explicit_modes_override_browser_preference_changes()
    {
        var repository = new InMemorySettingsRepository(
            new AppSettings(themeMode: ThemeMode.System));
        var state = new ThemeState(repository);

        await state.InitializeAsync(TestContext.Current.CancellationToken);
        await state.SetModeAsync(
            ThemeMode.Light,
            TestContext.Current.CancellationToken);

        Assert.False(state.IsDarkMode);
        Assert.False(state.ApplySystemPreference(true));
        Assert.False(state.IsDarkMode);

        await state.SetModeAsync(
            ThemeMode.Dark,
            TestContext.Current.CancellationToken);

        Assert.True(state.IsDarkMode);
        Assert.False(state.ApplySystemPreference(false));
        Assert.True(state.IsDarkMode);
    }

    [Fact]
    public async Task SetModeAsync_persists_only_the_theme_change()
    {
        var initialSettings = new AppSettings(
            onboardingCompleted: true,
            themeMode: ThemeMode.System,
            reminderEnabled: true,
            reminderTime: new TimeOnly(7, 30),
            energyEnabled: false,
            anxietyEnabled: true,
            focusEnabled: false,
            sleepEnabled: true);
        var repository = new InMemorySettingsRepository(initialSettings);
        var state = new ThemeState(repository);

        await state.SetModeAsync(
            ThemeMode.Dark,
            TestContext.Current.CancellationToken);

        var savedSettings = repository.SavedSettings
            ?? throw new Xunit.Sdk.XunitException("Settings were not saved.");
        Assert.True(savedSettings.OnboardingCompleted);
        Assert.Equal(ThemeMode.Dark, savedSettings.ThemeMode);
        Assert.True(savedSettings.ReminderEnabled);
        Assert.Equal(new TimeOnly(7, 30), savedSettings.ReminderTime);
        Assert.False(savedSettings.EnergyEnabled);
        Assert.True(savedSettings.AnxietyEnabled);
        Assert.False(savedSettings.FocusEnabled);
        Assert.True(savedSettings.SleepEnabled);
    }

    [Fact]
    public async Task SetModeAsync_updates_the_theme_before_persistence_completes()
    {
        var repository = new DelayedSettingsRepository(
            new AppSettings(themeMode: ThemeMode.System));
        var state = new ThemeState(repository);
        await state.InitializeAsync(TestContext.Current.CancellationToken);
        var changeCount = 0;
        state.Changed += (_, _) => changeCount++;

        var saveTask = state.SetModeAsync(
            ThemeMode.Dark,
            TestContext.Current.CancellationToken);
        await repository.SaveStarted.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        Assert.Equal(ThemeMode.Dark, state.Mode);
        Assert.True(state.IsDarkMode);
        Assert.Equal(1, changeCount);

        repository.ReleaseSave();
        await saveTask;
    }

    [Fact]
    public async Task SetModeAsync_rolls_back_when_persistence_fails()
    {
        var repository = new FailingSettingsRepository(
            new AppSettings(themeMode: ThemeMode.System));
        var state = new ThemeState(repository);
        await state.InitializeAsync(TestContext.Current.CancellationToken);
        var changeCount = 0;
        state.Changed += (_, _) => changeCount++;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            state.SetModeAsync(
                ThemeMode.Dark,
                TestContext.Current.CancellationToken));

        Assert.Equal(ThemeMode.System, state.Mode);
        Assert.False(state.IsDarkMode);
        Assert.Equal(2, changeCount);
    }

    [Fact]
    public async Task SetModeAsync_finishes_persistence_after_cancellation_begins()
    {
        var repository = new DelayedSettingsRepository(
            new AppSettings(themeMode: ThemeMode.System));
        var state = new ThemeState(repository);
        await state.InitializeAsync(TestContext.Current.CancellationToken);
        using var cancellation = new CancellationTokenSource();

        var saveTask = state.SetModeAsync(ThemeMode.Dark, cancellation.Token);
        await repository.SaveStarted.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        cancellation.Cancel();
        repository.ReleaseSave();
        await saveTask;

        Assert.Equal(ThemeMode.Dark, state.Mode);
        Assert.Equal(ThemeMode.Dark, repository.StoredSettings.ThemeMode);
    }

    private sealed class InMemorySettingsRepository(AppSettings initialSettings)
        : ISettingsRepository
    {
        public AppSettings StoredSettings { get; private set; } = initialSettings;

        public AppSettings? SavedSettings { get; private set; }

        public Task<AppSettings> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(StoredSettings);
        }

        public Task SaveAsync(
            AppSettings settings,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StoredSettings = settings;
            SavedSettings = settings;
            return Task.CompletedTask;
        }

        public Task ResetAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StoredSettings = new AppSettings();
            SavedSettings = null;
            return Task.CompletedTask;
        }
    }

    private sealed class DelayedSettingsRepository(AppSettings initialSettings)
        : ISettingsRepository
    {
        private readonly TaskCompletionSource<bool> _releaseSave =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public AppSettings StoredSettings { get; private set; } = initialSettings;

        public TaskCompletionSource<bool> SaveStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<AppSettings> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(StoredSettings);
        }

        public async Task SaveAsync(
            AppSettings settings,
            CancellationToken cancellationToken = default)
        {
            SaveStarted.TrySetResult(true);
            await _releaseSave.Task.WaitAsync(cancellationToken);
            StoredSettings = settings;
        }

        public Task ResetAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StoredSettings = new AppSettings();
            return Task.CompletedTask;
        }

        public void ReleaseSave()
        {
            _releaseSave.TrySetResult(true);
        }
    }

    private sealed class FailingSettingsRepository(AppSettings initialSettings)
        : ISettingsRepository
    {
        public Task<AppSettings> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(initialSettings);
        }

        public Task SaveAsync(
            AppSettings settings,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromException(
                new InvalidOperationException("The test persistence failed."));
        }

        public Task ResetAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    private sealed class FailingOnceSettingsRepository(AppSettings initialSettings)
        : ISettingsRepository
    {
        private int _loadAttempts;

        public Task<AppSettings> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Interlocked.Increment(ref _loadAttempts) == 1)
            {
                return Task.FromException<AppSettings>(
                    new InvalidOperationException("The test load failed."));
            }

            return Task.FromResult(initialSettings);
        }

        public Task SaveAsync(
            AppSettings settings,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task ResetAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }
}
