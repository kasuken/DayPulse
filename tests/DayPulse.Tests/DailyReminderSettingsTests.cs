using Bunit;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class DailyReminderSettingsTests : BunitContext
{
    public DailyReminderSettingsTests()
    {
        Services.AddMudServices();
        Services.AddDayPulseLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Reminder_save_preserves_the_latest_unrelated_preferences()
    {
        var repository = new SettingsRepository();
        Services.AddSingleton<ISettingsRepository>(repository);
        var cut = Render<DailyReminderSettings>();
        Assert.Contains("Closing the app stops them", cut.Markup);
        cut.Find("input[type='checkbox']").Change(true);

        repository.Current = new AppSettings(
            onboardingCompleted: true, themeMode: ThemeMode.Dark, focusEnabled: false);
        await cut.Find("button:contains('Save reminder')").ClickAsync(new());

        Assert.True(repository.Current.ReminderEnabled);
        Assert.True(repository.Current.OnboardingCompleted);
        Assert.Equal(ThemeMode.Dark, repository.Current.ThemeMode);
        Assert.False(repository.Current.FocusEnabled);
        Assert.Contains("Reminder preferences saved", cut.Markup);
        Assert.DoesNotContain(JSInterop.Invocations,
            invocation => invocation.Identifier == "requestPermission");
    }

    [Fact]
    public async Task Reminder_write_failure_is_not_reported_as_success()
    {
        var repository = new SettingsRepository { FailWrites = true };
        Services.AddSingleton<ISettingsRepository>(repository);
        var cut = Render<DailyReminderSettings>();
        cut.Find("input[type='checkbox']").Change(true);
        await cut.Find("button:contains('Save reminder')").ClickAsync(new());

        Assert.Contains("could not be loaded or saved", cut.Find("[role='alert']").TextContent);
        Assert.DoesNotContain("Reminder preferences saved", cut.Markup);
        Assert.False(repository.Current.ReminderEnabled);
        Assert.True(cut.Find("input[type='checkbox']").HasAttribute("checked"));
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
                throw new IOException("Storage unavailable.");
            }
            Current = settings;
            return Task.CompletedTask;
        }

        public Task ResetAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
