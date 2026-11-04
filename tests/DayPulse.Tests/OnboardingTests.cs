using Bunit;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class OnboardingTests : BunitContext
{
    public OnboardingTests()
    {
        Services.AddMudServices();
        Services.AddDayPulseLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Onboarding_explains_privacy_before_showing_metric_choices()
    {
        Services.AddSingleton<ISettingsRepository>(new SettingsRepository());
        var cut = Render<Onboarding>(parameters => parameters
            .Add(component => component.InitialSettings, new AppSettings()));

        Assert.Contains("Welcome to DayPulse", cut.Markup);
        Assert.Contains("not medical advice", cut.Markup);
        Assert.Empty(cut.FindAll("input"));

        cut.Find("button:contains('Continue')").Click();

        Assert.Contains("No accounts, analytics, trackers", cut.Markup);
        Assert.Contains("not an encrypted vault", cut.Markup);
        cut.Find("button:contains('Continue')").Click();
        Assert.Equal(4, cut.FindAll("input[type='checkbox']").Count);
    }

    [Fact]
    public async Task Completing_onboarding_persists_choices_and_preserves_latest_theme()
    {
        var repository = new SettingsRepository();
        Services.AddSingleton<ISettingsRepository>(repository);
        AppSettings? completed = null;
        var cut = Render<Onboarding>(parameters => parameters
            .Add(component => component.InitialSettings, new AppSettings(energyEnabled: false))
            .Add(component => component.OnCompleted, settings => completed = settings));
        cut.Find("button:contains('Continue')").Click();
        cut.Find("button:contains('Continue')").Click();

        // Simulate another tab changing a preference after onboarding loaded.
        repository.Current = new AppSettings(
            themeMode: ThemeMode.Dark, reminderEnabled: true, reminderTime: new TimeOnly(19, 30));
        await cut.Find("[data-testid='daypulse-onboarding-finish']").ClickAsync(new());

        Assert.NotNull(completed);
        Assert.True(repository.Current.OnboardingCompleted);
        Assert.False(repository.Current.EnergyEnabled);
        Assert.Equal(ThemeMode.Dark, repository.Current.ThemeMode);
        Assert.True(repository.Current.ReminderEnabled);
        Assert.Equal(new TimeOnly(19, 30), repository.Current.ReminderTime);
    }

    [Fact]
    public async Task Failed_save_keeps_choices_and_does_not_complete_until_retry_succeeds()
    {
        var repository = new SettingsRepository { FailWrites = true };
        Services.AddSingleton<ISettingsRepository>(repository);
        var completed = false;
        var cut = Render<Onboarding>(parameters => parameters
            .Add(component => component.InitialSettings, new AppSettings(focusEnabled: false))
            .Add(component => component.OnCompleted, _ => completed = true));
        cut.Find("button:contains('Continue')").Click();
        cut.Find("button:contains('Continue')").Click();
        await cut.Find("[data-testid='daypulse-onboarding-finish']").ClickAsync(new());

        Assert.False(completed);
        Assert.Contains("Your preferences could not be saved", cut.Find("[role='alert']").TextContent);
        Assert.False(cut.FindAll("input[type='checkbox']")[2].HasAttribute("checked"));

        repository.FailWrites = false;
        await cut.Find("[data-testid='daypulse-onboarding-finish']").ClickAsync(new());
        Assert.True(completed);
        Assert.False(repository.Current.FocusEnabled);
    }

    private sealed class SettingsRepository : ISettingsRepository
    {
        public AppSettings Current { get; set; } = new();
        public bool FailWrites { get; set; }

        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Current);

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            if (FailWrites)
            {
                throw new IOException("Browser storage is full.");
            }

            Current = settings;
            return Task.CompletedTask;
        }

        public Task ResetAsync(CancellationToken cancellationToken = default)
        {
            Current = new();
            return Task.CompletedTask;
        }
    }
}
