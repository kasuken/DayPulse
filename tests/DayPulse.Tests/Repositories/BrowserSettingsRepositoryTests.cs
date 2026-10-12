using System.Text.Json;
using DayPulse.Core.Domain;
using DayPulse.Infrastructure.Repositories;

namespace DayPulse.Tests.Repositories;

public sealed class BrowserSettingsRepositoryTests
{
    [Fact]
    public async Task SaveAndLoad_PersistsSettingsAcrossRepositoryInstances()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var storage = new InMemoryClientStorage();
        var settings = new AppSettings(
            onboardingCompleted: true,
            themeMode: ThemeMode.Dark,
            reminderEnabled: true,
            reminderTime: new TimeOnly(7, 30),
            energyEnabled: false,
            anxietyEnabled: true,
            focusEnabled: false,
            sleepEnabled: true);

        await new BrowserSettingsRepository(storage).SaveAsync(settings, cancellationToken);

        var loaded = await new BrowserSettingsRepository(storage).LoadAsync(cancellationToken);

        Assert.True(loaded.OnboardingCompleted);
        Assert.Equal(ThemeMode.Dark, loaded.ThemeMode);
        Assert.True(loaded.ReminderEnabled);
        Assert.Equal(new TimeOnly(7, 30), loaded.ReminderTime);
        Assert.False(loaded.EnergyEnabled);
        Assert.True(loaded.AnxietyEnabled);
        Assert.False(loaded.FocusEnabled);
        Assert.True(loaded.SleepEnabled);
    }

    [Fact]
    public async Task Reset_RemovesStoredSettingsAndRestoresDomainDefaults()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new BrowserSettingsRepository(new InMemoryClientStorage());

        await repository.SaveAsync(
            new AppSettings(onboardingCompleted: true),
            cancellationToken);
        await repository.ResetAsync(cancellationToken);

        var loaded = await repository.LoadAsync(cancellationToken);

        Assert.False(loaded.OnboardingCompleted);
        Assert.Equal(ThemeMode.System, loaded.ThemeMode);
    }

    [Fact]
    public async Task Save_RetriesWhenAnotherContextChangesTheStoredValue()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var storage = new InMemoryClientStorage();
        var repository = new BrowserSettingsRepository(storage);
        var competingSettings = new AppSettings(themeMode: ThemeMode.Light);
        var settings = new AppSettings(
            onboardingCompleted: true,
            themeMode: ThemeMode.Dark);

        storage.InjectedConditionalWriteKey = "daypulse.settings";
        storage.InjectedConditionalWriteValue = JsonSerializer.Serialize(
            competingSettings,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        await repository.SaveAsync(settings, cancellationToken);

        var loaded = await repository.LoadAsync(cancellationToken);
        Assert.True(loaded.OnboardingCompleted);
        Assert.Equal(ThemeMode.Dark, loaded.ThemeMode);
    }
}
