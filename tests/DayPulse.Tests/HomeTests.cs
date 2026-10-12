using Bunit;
using DayPulse.Infrastructure;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class HomeTests : BunitContext
{
    public HomeTests()
    {
        Services.AddMudServices();
        Services.AddInfrastructure();
        JSInterop.Mode = JSRuntimeMode.Loose;
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
