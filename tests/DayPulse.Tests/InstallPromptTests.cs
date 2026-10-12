using Bunit;
using DayPulse.App.Shared;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class InstallPromptTests : BunitContext
{
    public InstallPromptTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Standalone_apps_do_not_render_the_install_prompt()
    {
        SetupModule(new InstallPromptState(
            IsStandalone: true,
            IsIosSafari: false,
            IsAndroid: true,
            IsMobile: true,
            IsDismissed: false,
            CanPrompt: true));

        var cut = Render<InstallPrompt>();

        Assert.Empty(cut.FindAll("[data-testid='daypulse-install-prompt']"));
    }

    [Fact]
    public void Persisted_dismissal_hides_the_install_prompt()
    {
        SetupModule(new InstallPromptState(
            IsStandalone: false,
            IsIosSafari: false,
            IsAndroid: true,
            IsMobile: true,
            IsDismissed: true,
            CanPrompt: false));

        var cut = Render<InstallPrompt>();

        Assert.Empty(cut.FindAll("[data-testid='daypulse-install-prompt']"));
    }

    [Fact]
    public void Dont_show_again_persists_the_dismissal()
    {
        var module = SetupModule(new InstallPromptState(
            IsStandalone: false,
            IsIosSafari: true,
            IsAndroid: false,
            IsMobile: true,
            IsDismissed: false,
            CanPrompt: false));

        var cut = Render<InstallPrompt>();
        cut.WaitForElement("[data-testid='daypulse-install-prompt-dont-show-again']")
            .Click();

        cut.WaitForAssertion(() =>
            Assert.Empty(cut.FindAll("[data-testid='daypulse-install-prompt']")));

        var dismissal = module.Invocations.Single(invocation => invocation.Identifier == "dismiss");
        Assert.Equal(true, dismissal.Arguments.Single());
    }

    [Fact]
    public void Ios_safari_prompt_shows_share_and_home_screen_steps()
    {
        SetupModule(new InstallPromptState(
            IsStandalone: false,
            IsIosSafari: true,
            IsAndroid: false,
            IsMobile: true,
            IsDismissed: false,
            CanPrompt: false));

        var cut = Render<InstallPrompt>();
        var prompt = cut.WaitForElement("[data-testid='daypulse-install-prompt']");

        Assert.Contains("Share", prompt.TextContent);
        Assert.Contains("Add to Home Screen", prompt.TextContent);
        Assert.Empty(cut.FindAll("[data-testid='daypulse-install-prompt-install']"));
    }

    [Fact]
    public void Android_without_native_prompt_shows_menu_install_guidance()
    {
        SetupModule(new InstallPromptState(
            IsStandalone: false,
            IsIosSafari: false,
            IsAndroid: true,
            IsMobile: true,
            IsDismissed: false,
            CanPrompt: false));

        var cut = Render<InstallPrompt>();
        var prompt = cut.WaitForElement("[data-testid='daypulse-install-prompt']");

        Assert.Contains("browser menu", prompt.TextContent);
        Assert.Contains("Install app", prompt.TextContent);
        Assert.Empty(cut.FindAll("[data-testid='daypulse-install-prompt-install']"));
    }

    [Fact]
    public void Android_with_native_prompt_shows_install_action()
    {
        var module = SetupModule(new InstallPromptState(
            IsStandalone: false,
            IsIosSafari: false,
            IsAndroid: true,
            IsMobile: true,
            IsDismissed: false,
            CanPrompt: true));
        module
            .Setup<InstallPromptResult>("promptInstall", _ => true)
            .SetResult(new InstallPromptResult(WasAvailable: true, WasAccepted: true));

        var cut = Render<InstallPrompt>();
        cut.WaitForElement("[data-testid='daypulse-install-prompt-install']")
            .Click();

        cut.WaitForAssertion(() =>
            Assert.Empty(cut.FindAll("[data-testid='daypulse-install-prompt']")));
        Assert.Contains(
            module.Invocations,
            invocation => invocation.Identifier == "promptInstall");
    }

    [Fact]
    public void Native_prompt_failure_falls_back_to_menu_guidance()
    {
        var module = SetupModule(new InstallPromptState(
            IsStandalone: false,
            IsIosSafari: false,
            IsAndroid: true,
            IsMobile: true,
            IsDismissed: false,
            CanPrompt: true));
        module
            .Setup<InstallPromptResult>("promptInstall", _ => true)
            .SetResult(new InstallPromptResult(WasAvailable: false, WasAccepted: false));

        var cut = Render<InstallPrompt>();
        cut.WaitForElement("[data-testid='daypulse-install-prompt-install']")
            .Click();

        var prompt = cut.WaitForElement("[data-testid='daypulse-install-prompt']");
        Assert.Contains("browser menu", prompt.TextContent);
        Assert.Contains("Install app", prompt.TextContent);
        Assert.Empty(cut.FindAll("[data-testid='daypulse-install-prompt-install']"));
    }

    [Fact]
    public async Task Session_dismissal_is_not_reversed_by_a_late_install_event()
    {
        SetupModule(new InstallPromptState(
            IsStandalone: false,
            IsIosSafari: false,
            IsAndroid: true,
            IsMobile: true,
            IsDismissed: false,
            CanPrompt: false));

        var cut = Render<InstallPrompt>();
        cut.WaitForElement("[data-testid='daypulse-install-prompt-got-it']")
            .Click();

        await cut.InvokeAsync(() => cut.Instance.OnBeforeInstallPromptAvailable());

        Assert.Empty(cut.FindAll("[data-testid='daypulse-install-prompt']"));
    }

    [Fact]
    public void Install_prompt_module_contains_standalone_platform_and_lifecycle_guards()
    {
        var module = File.ReadAllText(GetRepositoryPath(
            "src/DayPulse.App/Shared/InstallPrompt.razor.js"));

        Assert.Contains("display-mode: standalone", module);
        Assert.Contains("navigator.standalone", module);
        Assert.Contains("beforeinstallprompt", module);
        Assert.Contains("navigator.userAgent", module);
        Assert.Contains("CriOS|FxiOS|EdgiOS|OPiOS|GSA", module);
        Assert.Contains("daypulse.install-prompt.dismissed.v1", module);
        Assert.Contains("removeEventListener", module);
        Assert.Contains("invokeMethodAsync", module);
        Assert.Contains("__dayPulseInstallPrompt", module);

        var index = File.ReadAllText(GetRepositoryPath(
            "src/DayPulse.App/wwwroot/index.html"));
        Assert.Contains(
            "src=\"js/daypulse-install-prompt-bootstrap.js\"",
            index);

        var bootstrap = File.ReadAllText(GetRepositoryPath(
            "src/DayPulse.App/wwwroot/js/daypulse-install-prompt-bootstrap.js"));
        Assert.Contains("beforeinstallprompt", bootstrap);
        Assert.Contains("__dayPulseInstallPrompt", bootstrap);
    }

    [Fact]
    public void Built_app_includes_the_collocated_install_prompt_module()
    {
#if DEBUG
        const string configuration = "Debug";
#else
        const string configuration = "Release";
#endif
        var manifestPath = GetRepositoryPath(
            $"src/DayPulse.App/bin/{configuration}/net10.0/wwwroot/service-worker-assets.js");

        Assert.Contains(
            "\"url\": \"Shared/InstallPrompt.razor.js\"",
            File.ReadAllText(manifestPath));
        Assert.Contains(
            "\"url\": \"js/daypulse-install-prompt-bootstrap.js\"",
            File.ReadAllText(manifestPath));
    }

    private BunitJSModuleInterop SetupModule(InstallPromptState state)
    {
        var module = JSInterop.SetupModule("./Shared/InstallPrompt.razor.js");
        module
            .Setup<InstallPromptState>("initialize", _ => true)
            .SetResult(state);
        module.SetupVoid("dismiss", _ => true).SetVoidResult();
        return module;
    }

    private static string GetRepositoryPath(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "DayPulse.sln")))
            {
                return Path.Combine(
                    directory.FullName,
                    relativePath.Replace('/', Path.DirectorySeparatorChar));
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the repository root from {AppContext.BaseDirectory}.");
    }
}
