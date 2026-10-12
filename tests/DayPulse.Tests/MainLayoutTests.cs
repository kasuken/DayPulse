using Bunit;
using DayPulse.App.Shared;
using DayPulse.Infrastructure;
using Bunit.TestDoubles;
using DayPulse.App.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class MainLayoutTests : BunitContext
{
    public MainLayoutTests()
    {
        Services.AddMudServices();
        Services.AddInfrastructure();
        Services.AddDayPulseLocalization();
        Services.AddScoped<ThemeState>();
        JSInterop.Mode = JSRuntimeMode.Loose;

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
    public void Bottom_navigation_has_the_primary_routes_and_accessible_labels()
    {
        var cut = RenderLayout();

        var links = cut.FindAll("nav[aria-label='Primary navigation'] a");

        Assert.Equal(4, links.Count);
        Assert.Equal(
            ["Today", "History", "Insights", "Settings"],
            links.Select(link => link.GetAttribute("aria-label")).ToArray());
        Assert.Equal(
            ["/", "/history", "/insights", "/settings"],
            links.Select(link => link.GetAttribute("href")).ToArray());
    }

    [Fact]
    public void Today_is_active_only_for_the_exact_root_route()
    {
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        navigation.NavigateTo("/");
        var cut = RenderLayout();

        var today = cut.Find("[data-testid='daypulse-nav-today']");
        var history = cut.Find("[data-testid='daypulse-nav-history']");

        Assert.Equal("page", today.GetAttribute("aria-current"));
        Assert.Contains("daypulse-bottom-nav__link--active", today.GetAttribute("class"));
        Assert.Null(history.GetAttribute("aria-current"));

        navigation.NavigateTo("/history/details");

        cut.WaitForAssertion(() =>
        {
            Assert.Null(cut.Find("[data-testid='daypulse-nav-today']").GetAttribute("aria-current"));
            Assert.Equal(
                "page",
                cut.Find("[data-testid='daypulse-nav-history']").GetAttribute("aria-current"));
        });
    }

    [Theory]
    [InlineData("/history/2026-10-12", "daypulse-nav-history")]
    [InlineData("/insights/monthly", "daypulse-nav-insights")]
    [InlineData("/settings/notifications", "daypulse-nav-settings")]
    public void Feature_routes_keep_their_primary_navigation_item_selected(
        string route,
        string selectedTestId)
    {
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        navigation.NavigateTo(route);
        var cut = RenderLayout();

        var selectedLink = cut.Find($"[data-testid='{selectedTestId}']");

        Assert.Equal("page", selectedLink.GetAttribute("aria-current"));
        Assert.Contains("daypulse-bottom-nav__link--active", selectedLink.GetAttribute("class"));
        Assert.Single(cut.FindAll("a[aria-current='page']"));
    }

    [Fact]
    public void Shell_keeps_content_and_navigation_in_distinct_landmarks()
    {
        var cut = RenderLayout();

        Assert.NotNull(cut.Find("header"));
        Assert.NotNull(cut.Find("main"));
        Assert.Single(cut.FindAll("nav"));
        Assert.NotNull(cut.Find("nav[aria-label='Primary navigation']"));
        Assert.NotNull(cut.Find("[data-testid='daypulse-shell-content']"));
    }

    private IRenderedComponent<MainLayout> RenderLayout()
    {
        return Render<MainLayout>(parameters => parameters
            .Add(
                layout => layout.Body,
                (RenderFragment)(builder =>
                    builder.AddMarkupContent(
                        0,
                        "<div data-testid=\"daypulse-shell-content\"><h1>Test content</h1></div>"))));
    }
}
