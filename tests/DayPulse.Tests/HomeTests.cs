using Bunit;
using DayPulse.App.Shared;
using DayPulse.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class HomeTests : BunitContext
{
    public HomeTests()
    {
        Services.AddMudServices();
        Services.AddInfrastructure();
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
    public void Root_route_renders_the_MudBlazor_app_shell()
    {
        var storageModule = JSInterop.SetupModule("./js/daypulse-storage.js");
        storageModule
            .Setup<bool>("localStorageTrySet", _ => true)
            .SetResult(true);

        var cut = Render<global::DayPulse.App.App>();

        Assert.Contains("DayPulse", cut.Find("[data-testid='daypulse-home']").TextContent);
        Assert.Equal("DayPulse", cut.Find("h1").TextContent);
        Assert.NotNull(cut.Find("header"));
    }
}
