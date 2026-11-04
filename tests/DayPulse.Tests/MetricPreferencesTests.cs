using Bunit;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure.Repositories;
using DayPulse.Tests.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class MetricPreferencesTests : BunitContext
{
    private static CancellationToken TestCancellation => Xunit.TestContext.Current.CancellationToken;

    public MetricPreferencesTests()
    {
        Services.AddMudServices();
        Services.AddDayPulseLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Toggling_a_metric_preserves_latest_theme_onboarding_and_other_preferences()
    {
        var repository = new BrowserSettingsRepository(new InMemoryClientStorage());
        await repository.SaveAsync(new AppSettings(), TestCancellation);
        Services.AddSingleton<ISettingsRepository>(repository);
        var cut = Render<MetricPreferences>();
        await repository.SaveAsync(new AppSettings(onboardingCompleted: true, themeMode: ThemeMode.Dark,
            reminderEnabled: true, reminderTime: new TimeOnly(9, 15), sleepEnabled: false), TestCancellation);

        await cut.InvokeAsync(() => cut.FindComponents<MudSwitch<bool>>()[0].Instance.ValueChanged.InvokeAsync(false));

        var saved = await repository.LoadAsync(TestCancellation);
        Assert.False(saved.EnergyEnabled);
        Assert.True(saved.OnboardingCompleted);
        Assert.Equal(ThemeMode.Dark, saved.ThemeMode);
        Assert.True(saved.ReminderEnabled);
        Assert.Equal(new TimeOnly(9, 15), saved.ReminderTime);
        Assert.False(saved.SleepEnabled);
        Assert.True(saved.AnxietyEnabled);
        Assert.True(saved.FocusEnabled);
    }

    [Fact]
    public async Task A_failed_metric_save_keeps_the_last_saved_selection_and_shows_an_error()
    {
        Services.AddSingleton<ISettingsRepository>(new FailingSaveSettingsRepository());
        var cut = Render<MetricPreferences>();

        await cut.InvokeAsync(() => cut.FindComponents<MudSwitch<bool>>()[0].Instance.ValueChanged.InvokeAsync(false));

        Assert.Contains("Metric preferences could not be loaded or saved", cut.Find("[role='alert']").TextContent);
        Assert.True(((AngleSharp.Html.Dom.IHtmlInputElement)cut.Find("input[type='checkbox']")).IsChecked);
    }

    private sealed class FailingSaveSettingsRepository : ISettingsRepository
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new AppSettings());

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) =>
            Task.FromException(new InvalidOperationException("Storage write failed."));

        public Task ResetAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
