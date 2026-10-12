using Bunit;
using DayPulse.App.Pages;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class DesignSystemComponentTests : BunitContext
{
    public DesignSystemComponentTests()
    {
        Services.AddMudServices();
        Services.AddDayPulseLocalization();
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
    public void History_clearly_labels_its_upcoming_workflow_and_has_a_return_link()
    {
        var cut = Render<History>();

        Assert.Equal("History", cut.Find("h1").TextContent);
        Assert.Contains("History is coming with daily check-ins.", cut.Markup);
        Assert.Contains("Coming next", cut.Markup);
        Assert.Equal("/", cut.Find("a").GetAttribute("href"));
    }

    [Fact]
    public void Insights_does_not_present_fabricated_statistics_as_real_data()
    {
        var cut = Render<Insights>();

        Assert.Equal("Insights", cut.Find("h1").TextContent);
        Assert.Contains("Trends and summaries are coming after daily check-ins.", cut.Markup);
        Assert.Contains("Coming next", cut.Markup);
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
