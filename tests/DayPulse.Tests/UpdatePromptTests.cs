using Bunit;
using DayPulse.App.Shared;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class UpdatePromptTests : BunitContext
{
    private const string ModulePath = "./Shared/UpdatePrompt.razor.js";

    public UpdatePromptTests()
    {
        Services.AddMudServices();
        Services.AddDayPulseLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void No_prompt_is_shown_when_no_update_is_waiting()
    {
        SetupModule(updateWaiting: false);

        Render<UpdatePrompt>();

        Assert.Empty(Services.GetRequiredService<ISnackbar>().ShownSnackbars);
    }

    [Fact]
    public void A_waiting_update_shows_one_persistent_reload_prompt()
    {
        SetupModule(updateWaiting: true);

        var provider = Render<MudSnackbarProvider>();
        Render<UpdatePrompt>();

        provider.WaitForAssertion(() =>
        {
            Assert.Contains("A new version of DayPulse is available.", provider.Markup);
            Assert.Single(ReloadButtons(provider));
        });
        Assert.Single(Services.GetRequiredService<ISnackbar>().ShownSnackbars);
    }

    [Fact]
    public void An_update_found_after_start_shows_the_prompt_once()
    {
        SetupModule(updateWaiting: false);
        var cut = Render<UpdatePrompt>();

        cut.InvokeAsync(() => cut.Instance.OnUpdateAvailable());
        cut.InvokeAsync(() => cut.Instance.OnUpdateAvailable());

        Assert.Single(Services.GetRequiredService<ISnackbar>().ShownSnackbars);
    }

    [Fact]
    public void Choosing_reload_activates_the_waiting_worker()
    {
        var module = SetupModule(updateWaiting: true);
        var provider = Render<MudSnackbarProvider>();
        Render<UpdatePrompt>();
        provider.WaitForAssertion(() =>
            Assert.Single(ReloadButtons(provider)));

        ReloadButtons(provider).Single().Click();

        provider.WaitForAssertion(() =>
            Assert.Contains(module.Invocations, invocation => invocation.Identifier == "applyUpdate"));
    }

    [Fact]
    public async Task Disposing_the_prompt_detaches_the_update_callback()
    {
        var module = SetupModule(updateWaiting: false);
        var cut = Render<UpdatePrompt>();

        await DisposeComponentsAsync();

        Assert.Contains(module.Invocations, invocation => invocation.Identifier == "dispose");
        Assert.NotNull(cut);
    }

    private static IReadOnlyList<AngleSharp.Dom.IElement> ReloadButtons(IRenderedComponent<MudSnackbarProvider> provider) =>
        [.. provider.FindAll("button").Where(button => button.TextContent.Trim() == "Reload")];

    private BunitJSModuleInterop SetupModule(bool updateWaiting)
    {
        var module = JSInterop.SetupModule(ModulePath);
        module.Setup<bool>("initialize", _ => true).SetResult(updateWaiting);
        module.SetupVoid("applyUpdate", _ => true).SetVoidResult();
        module.SetupVoid("dispose", _ => true).SetVoidResult();
        return module;
    }
}
