using Bunit;
using DayPulse.App.Pages;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Infrastructure;
using DayPulse.Infrastructure.Storage;
using DayPulse.Tests.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class DesignSystemComponentTests : BunitContext
{
    public DesignSystemComponentTests()
    {
        Services.AddMudServices();
        Services.AddDayPulseLocalization();
        Services.AddInfrastructure();
        Services.AddSingleton<IAtomicClientStorage>(new InMemoryClientStorage());
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(ThemeMode.System, "System Follow your device")]
    [InlineData(ThemeMode.Light, "Light A bright, clear canvas")]
    [InlineData(ThemeMode.Dark, "Dark A softer view after dark")]
    public async Task Theme_choice_has_a_name_state_and_a_working_selection_event(ThemeMode mode, string label)
    {
        ThemeMode? selection = null;
        var cut = Render<ThemeChoice>(parameters => parameters
            .Add(choice => choice.Mode, mode)
            .Add(choice => choice.Selected, true)
            .Add(choice => choice.OnSelected, selected => selection = selected));
        var button = cut.Find("button");

        Assert.Equal(label, button.GetAttribute("aria-label"));
        Assert.Equal("true", button.GetAttribute("aria-pressed"));
        await cut.InvokeAsync(() => button.Click());
        Assert.Equal(mode, selection);
    }

    [Fact]
    public void Disabled_theme_choice_cannot_change_the_preference()
    {
        var invoked = false;
        var cut = Render<ThemeChoice>(parameters => parameters
            .Add(choice => choice.Disabled, true)
            .Add(choice => choice.OnSelected, _ => invoked = true));

        Assert.True(cut.Find("button").HasAttribute("disabled"));
        cut.Find("button").Click();
        Assert.False(invoked);
    }

    [Fact]
    public void History_clearly_labels_its_empty_journal_and_has_a_check_in_link()
    {
        var cut = Render<History>();

        Assert.Equal("History", cut.Find("h1").TextContent);
        Assert.Contains("Your story starts here", cut.Markup);
        Assert.Contains("Save your first check-in", cut.Markup);
        Assert.Equal("/check-in", cut.Find("a").GetAttribute("href"));
    }

    [Fact]
    public void Insights_does_not_present_fabricated_statistics_as_real_data()
    {
        Services.AddSingleton<TimeProvider>(TimeProvider.System);
        var cut = Render<Insights>();

        Assert.Equal("Insights", cut.Find("h1").TextContent);
        Assert.Single(cut.FindAll("[data-testid='insights-empty']"));
        Assert.Contains("Save a check-in to see your personal summaries here.", cut.Markup);
        Assert.DoesNotContain("Coming next", cut.Markup);
        Assert.Empty(cut.FindAll("[data-testid='insights-period-picker']"));
        Assert.Empty(cut.FindAll("[data-testid='insights-mood-table']"));
        Assert.Equal("/", cut.Find("a").GetAttribute("href"));
        Assert.All(cut.FindAll("[role='img']"), icon =>
            Assert.NotNull(icon.Closest("[aria-hidden='true']")));
    }

    [Fact]
    public void Not_found_page_provides_a_working_route_back_to_the_application()
    {
        var cut = Render<NotFound>();

        Assert.Equal("Page not found", cut.Find("h1").TextContent);
        Assert.Equal("/", cut.Find("a").GetAttribute("href"));
        Assert.Equal("Back to today", cut.Find("a").TextContent.Trim());
    }
}
