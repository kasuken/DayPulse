using System.Globalization;
using System.Xml.Linq;
using Bunit;
using DayPulse.App;
using DayPulse.App.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DayPulse.Tests;

public sealed class LocalizationTests
{
    private static readonly string[] ResourceKeys =
    [
        "ApplicationName",
        "LocalStorageUnavailableTitle",
        "LocalStorageUnavailableMessage",
        "BootstrapUnhandledError",
        "BootstrapReload",
        "BootstrapErrorBoundary",
        "PrimaryNavigation",
        "SkipToMainContent",
        "Today",
        "History",
        "Insights",
        "Settings",
        "HomePageTitle",
        "HomeDescription",
        "HomeReadyHeading",
        "HomeReadyMessage",
        "HistoryPageTitle",
        "HistoryDescription",
        "InsightsPageTitle",
        "InsightsDescription",
        "PageNotFoundPageTitle",
        "PageNotFoundHeading",
        "PageNotFoundDescription",
        "SettingsPageTitle",
        "SettingsDescription",
        "Appearance",
        "ThemePreference",
        "ThemePreferenceHelper",
        "ThemeModeSystem",
        "ThemeModeLight",
        "ThemeModeDark",
        "LoadingThemePreference",
        "SavingThemePreference",
        "ThemePreferenceLoadError",
        "TryAgain",
        "Retrying",
        "RetryingThemePreference",
        "ThemePreferenceSaveError",
        "InstallDayPulse",
        "InstallDescription",
        "DismissInstallGuidance",
        "InstallIosDescription",
        "InstallIosShareStep",
        "InstallIosAddHomeStep",
        "InstallAndroidMenuDescription",
        "InstallAndroidMenuStep",
        "InstallAndroidAddStep",
        "InstallAndroidPromptDescription",
        "Install",
        "GotIt",
        "DontShowAgain",
        "OnDevicePrivate",
        "PrivacyHeading",
        "PrivacyDescription",
        "ComingNext",
        "CheckInsComingSoon",
        "PersonalizeAppearance",
        "YourDayYourPace",
        "WellbeingHeading",
        "WellbeingDescription",
        "MetricMood",
        "MetricEnergy",
        "MetricAnxiety",
        "MetricFocus",
        "MetricSleep",
        "HistoryPreviewHeading",
        "HistoryPreviewDescription",
        "InsightsPreviewHeading",
        "InsightsPreviewDescription",
        "ViewHistory",
        "ExploreInsights",
        "BackToToday",
        "HistoryEmptyHeading",
        "HistoryEmptyDescription",
        "InsightsEmptyHeading",
        "InsightsEmptyDescription",
        "ThemeModeSystemDescription",
        "ThemeModeLightDescription",
        "ThemeModeDarkDescription",
    ];

    [Fact]
    public void AddDayPulseLocalization_registers_localizer_culture_and_formatter_services()
    {
        var services = new ServiceCollection();
        services.AddDayPulseLocalization();
        services.AddLogging();

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IStringLocalizer<SharedResource>>());
        Assert.IsType<CultureSelector>(provider.GetRequiredService<CultureSelector>());
        Assert.IsType<CultureState>(provider.GetRequiredService<CultureState>());
        Assert.IsType<LocalizedFormatService>(
            provider.GetRequiredService<LocalizedFormatService>());
    }

    [Theory]
    [InlineData("en-US", "en-US")]
    [InlineData("en-GB", "en-US")]
    [InlineData("de-DE", "en-US")]
    [InlineData("it-IT", "en-US")]
    [InlineData("fr-FR", "en-US")]
    [InlineData("not-a-culture", "en-US")]
    public void Culture_selector_keeps_unsupported_locales_on_the_default(
        string browserLocale,
        string expectedCultureName)
    {
        var selector = new CultureSelector();

        var culture = selector.SelectCulture([browserLocale]);

        Assert.Equal(expectedCultureName, culture.Name);
    }

    [Fact]
    public void Culture_selector_uses_the_first_supported_browser_preference()
    {
        var selector = new CultureSelector();

        var culture = selector.SelectCulture(["fr-FR", "en-US"]);

        Assert.Equal(CultureSelector.DefaultCultureName, culture.Name);
    }

    [Fact]
    public void Culture_selector_falls_back_when_browser_locale_information_is_unavailable()
    {
        var selector = new CultureSelector();

        var culture = selector.SelectCulture(null);

        Assert.Equal(CultureSelector.DefaultCultureName, culture.Name);
    }

    [Fact]
    public void Culture_state_applies_default_culture_to_current_and_ui_culture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        var previousDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        var previousDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;

        try
        {
            var state = new CultureState(new CultureSelector());

            Assert.Equal(CultureSelector.DefaultCultureName, state.CurrentCulture.Name);
            Assert.Equal(CultureSelector.DefaultCultureName, CultureInfo.CurrentCulture.Name);
            Assert.Equal(CultureSelector.DefaultCultureName, CultureInfo.CurrentUICulture.Name);
            Assert.Equal(
                CultureSelector.DefaultCultureName,
                CultureInfo.DefaultThreadCurrentCulture?.Name);
            Assert.Equal(
                CultureSelector.DefaultCultureName,
                CultureInfo.DefaultThreadCurrentUICulture?.Name);

            Assert.False(state.ApplyBrowserLocales(["de-DE"]));
            Assert.Equal(CultureSelector.DefaultCultureName, state.CurrentCulture.Name);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
            CultureInfo.DefaultThreadCurrentCulture = previousDefaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = previousDefaultUiCulture;
        }
    }

    [Fact]
    public void Shared_resource_localizer_resolves_the_neutral_english_resource()
    {
        using var provider = CreateLocalizationProvider();
        var localizer = provider.GetRequiredService<IStringLocalizer<SharedResource>>();
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            var localized = localizer["HomeDescription"];

            Assert.False(localized.ResourceNotFound);
            Assert.Equal("Your private wellbeing companion.", localized.Value);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Fact]
    public void Shared_resource_localizer_resolves_the_matching_culture_fallback_resource()
    {
        using var provider = CreateLocalizationProvider();
        var localizer = provider.GetRequiredService<IStringLocalizer<SharedResource>>();
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");

            var localized = localizer["HomeDescription"];

            Assert.False(localized.ResourceNotFound);
            Assert.Equal("Your private wellbeing companion.", localized.Value);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Theory]
    [InlineData("de")]
    [InlineData("it")]
    public void Culture_specific_resource_sets_are_embedded(
        string cultureName)
    {
        using var provider = CreateLocalizationProvider();
        var localizer = provider.GetRequiredService<IStringLocalizer<SharedResource>>();
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);

            var cultureSpecificStrings = localizer
                .GetAllStrings(includeParentCultures: false)
                .ToArray();

            Assert.Contains(
                cultureSpecificStrings,
                localized => localized.Name == "HomeDescription");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Fact]
    public void Localized_format_service_uses_current_culture_without_changing_date_only()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            var formatter = new LocalizedFormatService();
            var date = new DateOnly(2026, 10, 12);

            Assert.Equal("12.10.2026", formatter.FormatDate(date));
            Assert.Equal(
                "12.10.2026 15:37",
                formatter.FormatDateTime(new DateTime(2026, 10, 12, 15, 37, 0)));
            Assert.Equal(
                "12.10.2026 15:37",
                formatter.FormatDateTime(
                    new DateTimeOffset(2026, 10, 12, 15, 37, 0, TimeSpan.FromHours(2))));
            Assert.Equal("1.234,56", formatter.FormatNumber(1234.56m, "N2"));
            Assert.Equal(new DateOnly(2026, 10, 12), date);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public async Task Browser_locale_initializer_reads_browser_locales_sets_document_language_and_disposes()
    {
        await using var context = new BunitContext();
        context.Services.AddDayPulseLocalization();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var module = context.JSInterop.SetupModule(
            "./Shared/BrowserLocaleInitializer.razor.js");
        module
            .Setup<string[]?>("getBrowserLocales", _ => true)
            .SetResult(["de-DE"]);
        module
            .SetupVoid("setDocumentLanguage", _ => true)
            .SetVoidResult();
        module
            .SetupVoid("dispose", _ => true)
            .SetVoidResult();

        var cut = context.Render<BrowserLocaleInitializer>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains(
                module.Invocations,
                invocation => invocation.Identifier == "getBrowserLocales");
            Assert.Contains(
                module.Invocations,
                invocation => invocation.Identifier == "setDocumentLanguage"
                    && invocation.Arguments.Single() is "en-US");
            Assert.Contains(
                module.Invocations,
                invocation => invocation.Identifier == "setBootstrapErrorText");
        });

        await cut.Instance.DisposeAsync();

        Assert.Contains(
            module.Invocations,
            invocation => invocation.Identifier == "dispose");
    }

    [Fact]
    public void Culture_specific_resource_files_keep_the_same_keys_as_the_neutral_resource()
    {
        var resourceDirectory = GetRepositoryPath("src/DayPulse.App/Resources");
        var neutralKeys = ReadResourceKeys(
            Path.Combine(resourceDirectory, "SharedResource.resx"));

        foreach (var culture in new[] { "de", "it" })
        {
            var cultureKeys = ReadResourceKeys(
                Path.Combine(resourceDirectory, $"SharedResource.{culture}.resx"));
            Assert.Equal(neutralKeys, cultureKeys);
        }

        Assert.Equal(ResourceKeys, neutralKeys);
    }

    [Fact]
    public void Browser_locale_module_uses_browser_preferences_and_updates_document_language()
    {
        var module = File.ReadAllText(
            GetRepositoryPath("src/DayPulse.App/Shared/BrowserLocaleInitializer.razor.js"));

        Assert.Contains("browserNavigator.languages", module);
        Assert.Contains("browserNavigator.language", module);
        Assert.Contains("document.documentElement.lang", module);
        Assert.Contains("blazor-error-message", module);
        Assert.Contains("blazor-error-reload", module);
        Assert.Contains("setBootstrapErrorText", module);
        Assert.Contains("export function dispose", module);
    }

    [Fact]
    public void WebAssembly_loads_globalization_data_to_allow_the_startup_culture_fallback()
    {
        var project = XDocument.Load(GetRepositoryPath("src/DayPulse.App/DayPulse.App.csproj"));

        Assert.Equal("true", project.Root?.Elements("PropertyGroup")
            .Elements("BlazorWebAssemblyLoadAllGlobalizationData")
            .Single().Value);
    }

    [Fact]
    public void Razor_components_reference_resources_instead_of_current_user_facing_literals()
    {
        var appDirectory = GetRepositoryPath("src/DayPulse.App");
        var razorFiles = Directory.EnumerateFiles(
                appDirectory,
                "*.razor",
                SearchOption.AllDirectories)
            .Select(File.ReadAllText)
            .ToArray();
        var razorMarkupSource = string.Join(
            Environment.NewLine,
            razorFiles.Select(source => source.Split("@code", 2)[0]));
        var razorSource = string.Join(
            Environment.NewLine,
            razorFiles);

        foreach (var userFacingLiteral in new[]
        {
            "Local storage is unavailable",
            "DayPulse could not initialize its local data. Refresh the page and try again.",
            "Primary navigation",
            "Your private wellbeing companion.",
            "Ready when you are",
            "Your daily check-in will live here.",
            "History | DayPulse",
            "Your past check-ins will appear here.",
            "Insights | DayPulse",
            "Your wellbeing patterns will appear here.",
            "Page not found",
            "Sorry, the content you are looking for does not exist.",
            "Settings | DayPulse",
            "Your DayPulse preferences will appear here.",
            "Theme preference",
            "System follows your device preference.",
            "Loading theme preference…",
            "Saving theme preference…",
            "The theme preference could not be loaded.",
            "Try again",
            "Retrying…",
            "Retrying theme preference…",
            "The theme preference could not be saved.",
            "Install DayPulse",
            "Keep your daily check-ins close at hand.",
            "Dismiss install guidance",
            "Add DayPulse to your Home Screen for a faster, app-like experience:",
            "Tap <strong>Share</strong> in Safari.",
            "Choose <strong>Add to Home Screen</strong>, then tap <strong>Add</strong>.",
            "Add DayPulse to your Home Screen from your browser menu:",
            "Open the browser menu using <strong>⋮</strong>.",
            "Tap <strong>Install app</strong> or <strong>Add to Home screen</strong>.",
            "Install DayPulse for quick access and a focused app experience.",
            "Don't show again",
        })
        {
            Assert.DoesNotContain(
                userFacingLiteral,
                razorMarkupSource,
                StringComparison.Ordinal);
        }

        foreach (var resourceKey in ResourceKeys)
        {
            Assert.Contains($"\"{resourceKey}\"", razorSource, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Design_copy_is_not_embedded_in_Razor_markup()
    {
        var appDirectory = GetRepositoryPath("src/DayPulse.App");
        var markup = string.Join(Environment.NewLine,
            Directory.EnumerateFiles(appDirectory, "*.razor", SearchOption.AllDirectories)
                .Select(path => File.ReadAllText(path).Split("@code", 2)[0]));
        var resources = XDocument.Load(Path.Combine(appDirectory, "Resources/SharedResource.resx"));

        foreach (var resource in resources.Root!.Elements("data"))
        {
            var key = resource.Attribute("name")!.Value;
            var text = resource.Element("value")!.Value;
            Assert.False(string.IsNullOrWhiteSpace(text), $"Missing copy for {key}.");
            if (text.Length > 12)
            {
                Assert.DoesNotContain(text, markup, StringComparison.Ordinal);
            }
        }
    }

    private static ServiceProvider CreateLocalizationProvider()
    {
        var services = new ServiceCollection();
        services.AddDayPulseLocalization();
        services.AddLogging();
        return services.BuildServiceProvider();
    }

    private static string[] ReadResourceKeys(string path)
    {
        return XDocument.Load(path)
            .Root?
            .Elements("data")
            .Select(element => element.Attribute("name")?.Value)
            .Where(name => name is not null)
            .Cast<string>()
            .ToArray()
            ?? [];
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
