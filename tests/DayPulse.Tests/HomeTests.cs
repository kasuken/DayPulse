using Bunit;
using DayPulse.App.Shared;
using DayPulse.Infrastructure;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure.Storage;
using DayPulse.Tests.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class HomeTests : BunitContext
{
    public HomeTests()
    {
        Services.AddMudServices();
        Services.AddInfrastructure();
        Services.AddSingleton<IAtomicClientStorage>(new InMemoryClientStorage());
        Services.AddSingleton<TimeProvider>(new FixedJournalTimeProvider());
        Services.AddDayPulseLocalization();
        Services.AddScoped<ThemeState>();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var localeModule = JSInterop.SetupModule(
            "./Shared/BrowserLocaleInitializer.razor.js");
        localeModule
            .Setup<string[]?>("getBrowserLocales", _ => true)
            .SetResult(["en-US"]);
        localeModule
            .SetupVoid("setDocumentLanguage", _ => true)
            .SetVoidResult();
        localeModule
            .SetupVoid("dispose", _ => true)
            .SetVoidResult();

        var themeModule = JSInterop.SetupModule("./Shared/ThemeProvider.razor.js");
        themeModule
            .Setup<bool>("initialize", _ => true)
            .SetResult(false);
        themeModule
            .Setup<bool>("setMode", _ => true)
            .SetResult(false);
        themeModule
            .SetupVoid("dispose", _ => true)
            .SetVoidResult();
    }

    [Fact]
    public async Task Root_route_renders_the_MudBlazor_app_shell()
    {
        await Services.GetRequiredService<ISettingsRepository>()
            .SaveAsync(new AppSettings(onboardingCompleted: true), Xunit.TestContext.Current.CancellationToken);
        var storageModule = JSInterop.SetupModule("./js/daypulse-storage.js");
        storageModule
            .Setup<bool>("storageTrySet", _ => true)
            .SetResult(true);

        var cut = Render<global::DayPulse.App.App>();

        cut.WaitForAssertion(() =>
            Assert.Contains("Make room for how you feel.", cut.Find("[data-testid='daypulse-home']").TextContent));
        Assert.Equal("Today", cut.Find("h1").TextContent);
        Assert.NotNull(cut.Find("header"));
    }

    [Fact]
    public void Home_links_to_available_routes_and_offers_a_real_check_in()
    {
        var cut = Render<global::DayPulse.App.Pages.Home>();

        cut.WaitForAssertion(() =>
            Assert.Equal("/settings", cut.Find("a:contains('Personalize appearance')").GetAttribute("href")));
        Assert.Equal("/history", cut.Find("a:contains('View history')").GetAttribute("href"));
        Assert.Equal("/insights", cut.Find("a:contains('Explore insights')").GetAttribute("href"));
        Assert.Equal("/check-in", cut.Find("a:contains('Check in')").GetAttribute("href"));
        Assert.Contains("Nothing is recorded until you save.", cut.Markup);
        Assert.Empty(cut.FindAll("input"));
        Assert.Empty(cut.FindAll("[role='slider']"));
        Assert.Equal(5, cut.FindAll(".daypulse-metric-overview__item").Count);
    }

    [Fact]
    public async Task Home_uses_the_local_calendar_date_and_shows_the_saved_entry()
    {
        await Services.GetRequiredService<ICheckInRepository>().CreateAsync(
            new CheckIn("today", FixedJournalTimeProvider.Today, 4, energy: 7), Xunit.TestContext.Current.CancellationToken);

        var cut = Render<global::DayPulse.App.Pages.Home>();

        cut.WaitForAssertion(() =>
            Assert.Equal("/history/today", cut.Find("a:contains('View check-in')").GetAttribute("href")));
        Assert.Contains("Today's check-in is saved", cut.Markup);
        Assert.Contains("7", cut.FindAll(".daypulse-metric-overview__item")[1].TextContent);
        Assert.Empty(cut.FindAll("a[href='/check-in']"));
    }

    [Fact]
    public async Task Home_hides_metrics_disabled_in_settings()
    {
        await Services.GetRequiredService<ISettingsRepository>()
            .SaveAsync(new AppSettings(energyEnabled: false, sleepEnabled: false), Xunit.TestContext.Current.CancellationToken);

        var cut = Render<global::DayPulse.App.Pages.Home>();

        cut.WaitForAssertion(() =>
            Assert.Equal(3, cut.FindAll(".daypulse-metric-overview__item").Count));
        Assert.DoesNotContain("Energy", cut.FindComponent<global::DayPulse.App.Pages.Components.WellbeingOverview>().Markup);
    }
}
