using Bunit;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class ThemeProviderTests : BunitContext
{
    public ThemeProviderTests()
    {
        Services.AddMudServices();
        Services.AddScoped<ThemeState>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Provider_restores_the_saved_mode_during_first_render()
    {
        Services.AddSingleton<ISettingsRepository>(
            new InMemorySettingsRepository(new AppSettings(themeMode: ThemeMode.Dark)));
        var module = SetupThemeModule(isDarkMode: false);

        var cut = Render<ThemeProvider>();

        Assert.Equal(ThemeMode.Dark, Services.GetRequiredService<ThemeState>().Mode);
        Assert.True(cut.FindComponent<MudThemeProvider>().Instance
            .GetState(themeProvider => themeProvider.IsDarkMode));
        var initialization = Assert.Single(
            module.Invocations,
            invocation => invocation.Identifier == "initialize");
        Assert.Equal("Dark", initialization.Arguments[0]);
    }

    [Fact]
    public async Task Provider_reinitializes_after_theme_state_recovers_from_load_failure()
    {
        Services.AddSingleton<ISettingsRepository>(
            new FailingOnceSettingsRepository(new AppSettings(themeMode: ThemeMode.Dark)));
        var module = SetupThemeModule(isDarkMode: false);
        var cut = Render<ThemeProvider>();
        var state = Services.GetRequiredService<ThemeState>();

        cut.WaitForAssertion(() =>
        {
            Assert.True(state.IsInitializationFailed);
            Assert.Empty(module.Invocations);
        });

        await cut.InvokeAsync(() =>
            state.InitializeAsync(Xunit.TestContext.Current.CancellationToken));

        cut.WaitForAssertion(() =>
        {
            Assert.True(state.IsInitialized);
            Assert.False(state.IsInitializationFailed);
            Assert.True(cut.FindComponent<MudThemeProvider>().Instance
                .GetState(themeProvider => themeProvider.IsDarkMode));
            Assert.Contains(
                module.Invocations,
                invocation => invocation.Identifier == "initialize"
                    && invocation.Arguments[0] is "Dark");
        });
    }

    [Fact]
    public async Task Provider_retries_after_failed_js_initialization_and_cleans_up_before_replacement()
    {
        Services.AddSingleton<ISettingsRepository>(
            new InMemorySettingsRepository(new AppSettings(themeMode: ThemeMode.System)));
        var module = JSInterop.SetupModule("./Shared/ThemeProvider.razor.js");
        var initializeHandler = module
            .Setup<bool>("initialize", _ => true)
            .SetException(new InvalidOperationException("The test module failed."));
        module
            .Setup<bool>("setMode", _ => true)
            .SetResult(true);
        module
            .SetupVoid("dispose", _ => true)
            .SetVoidResult();

        var cut = Render<ThemeProvider>();
        var state = Services.GetRequiredService<ThemeState>();

        cut.WaitForAssertion(() =>
        {
            Assert.True(state.IsInitializationFailed);
            Assert.True(state.IsProviderInitializationFailed);
            Assert.False(state.IsProviderInitialized);
            Assert.Single(
                module.Invocations,
                invocation => invocation.Identifier == "initialize");
            Assert.Contains(
                module.Invocations,
                invocation => invocation.Identifier == "dispose");
        });

        initializeHandler.SetResult(true);
        await cut.InvokeAsync(() =>
            state.RetryInitializationAsync(Xunit.TestContext.Current.CancellationToken));

        cut.WaitForAssertion(() =>
        {
            Assert.True(state.IsInitialized);
            Assert.False(state.IsInitializationFailed);
            Assert.True(state.IsProviderInitialized);
            Assert.True(cut.FindComponent<MudThemeProvider>().Instance
                .GetState(themeProvider => themeProvider.IsDarkMode));
            Assert.Equal(
                2,
                module.Invocations.Count(invocation => invocation.Identifier == "initialize"));
        });

        Assert.Equal(
            1,
            module.Invocations.Count(invocation => invocation.Identifier == "dispose"));
    }

    [Fact]
    public async Task System_preference_changes_update_the_active_palette_without_reload()
    {
        Services.AddSingleton<ISettingsRepository>(
            new InMemorySettingsRepository(new AppSettings(themeMode: ThemeMode.System)));
        SetupThemeModule(isDarkMode: false);
        var cut = Render<ThemeProvider>();

        await cut.InvokeAsync(() => cut.Instance.OnSystemThemeChanged(true));

        cut.WaitForAssertion(() =>
        {
            Assert.True(cut.FindComponent<MudThemeProvider>().Instance
                .GetState(themeProvider => themeProvider.IsDarkMode));
            Assert.True(Services.GetRequiredService<ThemeState>().IsDarkMode);
        });

        await cut.InvokeAsync(() => cut.Instance.OnSystemThemeChanged(false));

        cut.WaitForAssertion(() =>
        {
            Assert.False(cut.FindComponent<MudThemeProvider>().Instance
                .GetState(themeProvider => themeProvider.IsDarkMode));
            Assert.False(Services.GetRequiredService<ThemeState>().IsDarkMode);
        });
    }

    [Fact]
    public async Task Changing_mode_applies_the_new_mode_without_reload()
    {
        Services.AddSingleton<ISettingsRepository>(
            new InMemorySettingsRepository(new AppSettings(themeMode: ThemeMode.System)));
        var module = SetupThemeModule(isDarkMode: false);
        var cut = Render<ThemeProvider>();
        var state = Services.GetRequiredService<ThemeState>();

        await cut.InvokeAsync(() => state.SetModeAsync(
            ThemeMode.Dark,
            Xunit.TestContext.Current.CancellationToken));

        cut.WaitForAssertion(() =>
        {
            Assert.True(cut.FindComponent<MudThemeProvider>().Instance
                .GetState(themeProvider => themeProvider.IsDarkMode));
            Assert.Contains(
                module.Invocations,
                invocation => invocation.Identifier == "setMode"
                    && invocation.Arguments[0] is "Dark");
        });
    }

    [Fact]
    public async Task Disposing_provider_cleans_up_the_theme_module()
    {
        Services.AddSingleton<ISettingsRepository>(
            new InMemorySettingsRepository(new AppSettings(themeMode: ThemeMode.Dark)));
        var module = SetupThemeModule(isDarkMode: false);
        var cut = Render<ThemeProvider>();

        await cut.Instance.DisposeAsync();

        var invocationNames = module.Invocations
            .Select(invocation => invocation.Identifier)
            .ToArray();
        Assert.Contains("dispose", invocationNames);
        Assert.True(
            Array.IndexOf(invocationNames, "initialize")
                < Array.IndexOf(invocationNames, "dispose"));
    }

    [Fact]
    public void Theme_module_subscribes_to_system_changes_and_cleans_up()
    {
        var modulePath = GetRepositoryPath(
            "src/DayPulse.App/Shared/ThemeProvider.razor.js");
        var module = File.ReadAllText(modulePath);

        Assert.Contains("prefers-color-scheme: dark", module);
        Assert.Contains("matchMedia", module);
        Assert.Contains("addEventListener", module);
        Assert.Contains("removeEventListener", module);
        Assert.Contains("invokeMethodAsync", module);
        Assert.Contains("console.error", module);
        Assert.Contains("notificationChain", module);
        Assert.Contains("await notificationChain", module);
        Assert.Contains("export async function dispose", module);
    }

    private BunitJSModuleInterop SetupThemeModule(bool isDarkMode)
    {
        var module = JSInterop.SetupModule("./Shared/ThemeProvider.razor.js");
        module
            .Setup<bool>("initialize", _ => true)
            .SetResult(isDarkMode);
        module
            .Setup<bool>("setMode", _ => true)
            .SetResult(isDarkMode);
        module
            .SetupVoid("dispose", _ => true)
            .SetVoidResult();
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

    private sealed class InMemorySettingsRepository(AppSettings initialSettings)
        : ISettingsRepository
    {
        public Task<AppSettings> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(initialSettings);
        }

        public Task SaveAsync(
            AppSettings settings,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task ResetAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    private sealed class FailingOnceSettingsRepository(AppSettings initialSettings)
        : ISettingsRepository
    {
        private int _loadAttempts;

        public Task<AppSettings> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Interlocked.Increment(ref _loadAttempts) == 1)
            {
                return Task.FromException<AppSettings>(
                    new InvalidOperationException("The test load failed."));
            }

            return Task.FromResult(initialSettings);
        }

        public Task SaveAsync(
            AppSettings settings,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task ResetAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }
}
