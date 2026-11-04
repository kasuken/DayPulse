using Bunit;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure;
using DayPulse.Infrastructure.Storage;
using DayPulse.Tests.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class ApplicationStartupTests : BunitContext
{
    public ApplicationStartupTests()
    {
        Services.AddMudServices();
        Services.AddInfrastructure();
        Services.AddDayPulseLocalization();
        Services.AddScoped<ThemeState>();
        Services.AddSingleton(TimeProvider.System);
        var storage = new InMemoryClientStorage();
        Services.AddSingleton<IClientStorage>(storage);
        Services.AddSingleton<IAtomicClientStorage>(storage);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task First_run_gates_the_journal_until_preferences_are_saved()
    {
        var cut = Render<global::DayPulse.App.App>();

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find("[data-testid='daypulse-onboarding']")));
        Assert.Empty(cut.FindAll("[data-testid='daypulse-home']"));
        await cut.Find("button:contains('Continue')").ClickAsync(new());
        cut.WaitForAssertion(() => Assert.Contains("Step 2 of 3", cut.Markup));
        await cut.Find("button:contains('Continue')").ClickAsync(new());
        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find("[data-testid='daypulse-onboarding-finish']")));
        await cut.Find("[data-testid='daypulse-onboarding-finish']").ClickAsync(new());

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find("[data-testid='daypulse-home']")));
        Assert.True((await Services.GetRequiredService<ISettingsRepository>()
            .LoadAsync(Xunit.TestContext.Current.CancellationToken)).OnboardingCompleted);
    }

    [Fact]
    public async Task Completed_onboarding_is_not_repeated_after_another_app_start()
    {
        await Services.GetRequiredService<ISettingsRepository>().SaveAsync(
            new AppSettings(onboardingCompleted: true), Xunit.TestContext.Current.CancellationToken);
        var cut = Render<global::DayPulse.App.App>();

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find("[data-testid='daypulse-home']")));
        Assert.Empty(cut.FindAll("[data-testid='daypulse-onboarding']"));
    }

    [Fact]
    public void Storage_initialization_failure_can_be_retried_without_reloading()
    {
        var repository = new UnavailableSettingsRepository();
        Services.AddSingleton<ISettingsRepository>(repository);
        var cut = Render<global::DayPulse.App.App>();
        cut.WaitForAssertion(() =>
            Assert.Contains("Local storage is unavailable", cut.Find("main[role='alert']").TextContent));
        Assert.Empty(cut.FindAll("[data-testid='daypulse-home']"));

        repository.Available = true;
        cut.Find("button:contains('Try again')").Click();
        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find("[data-testid='daypulse-onboarding']")));
    }

    private sealed class UnavailableSettingsRepository : ISettingsRepository
    {
        public bool Available { get; set; }
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
            => Available ? Task.FromResult(new AppSettings()) : throw new IOException("Storage unavailable.");
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task ResetAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
