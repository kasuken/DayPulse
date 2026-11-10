using Bunit;
using DayPulse.App.Pages;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class SettingsTests : BunitContext
{
    public SettingsTests()
    {
        Services.AddMudServices();
        Services.AddDayPulseLocalization();
        var storage = new DayPulse.Tests.Repositories.InMemoryClientStorage();
        Services.AddSingleton<IActivityRepository>(
            new DayPulse.Infrastructure.Repositories.BrowserActivityRepository(storage));
        Services.AddSingleton<ICheckInRepository>(
            new DayPulse.Infrastructure.Repositories.BrowserCheckInRepository(storage));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Section_shortcuts_target_focusable_settings_panels()
    {
        Services.AddSingleton<ISettingsRepository>(new InMemorySettingsRepository(new AppSettings()));
        Services.AddScoped<ThemeState>();
        var cut = Render<Settings>();
        await cut.WaitForElementAsync("#settings-metrics");

        var shortcuts = cut.FindAll("[data-testid='settings-shortcuts'] a");
        Assert.Equal(6, shortcuts.Count);
        foreach (var target in shortcuts.Select(shortcut => shortcut.GetAttribute("href")).ToArray())
        {
            Assert.NotNull(target);
            Assert.StartsWith("/settings#", target);
            Assert.Equal("-1", cut.Find(target["/settings".Length..]).GetAttribute("tabindex"));
            await cut.InvokeAsync(() => cut.Find($"a[href='{target}']").Click());
        }
        Assert.Equal(6, JSInterop.Invocations.Count(invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus"));
    }

    [Fact]
    public async Task Settings_exposes_an_accessible_theme_selector_with_the_current_choice()
    {
        var repository = new InMemorySettingsRepository(
            new AppSettings(themeMode: ThemeMode.Dark));
        Services.AddSingleton<ISettingsRepository>(repository);
        Services.AddScoped<ThemeState>();

        await Services.GetRequiredService<ThemeState>()
            .InitializeAsync(Xunit.TestContext.Current.CancellationToken);
        var cut = Render<Settings>();
        var selector = cut.Find("[role='group'][aria-label='Theme preference']");
        Assert.Equal(3, selector.QuerySelectorAll("button").Length);
        Assert.Equal("true", cut.Find("[data-testid='daypulse-theme-dark']").GetAttribute("aria-pressed"));
        Assert.Single(cut.FindAll("button[aria-pressed='true']"));
        Assert.Contains("System follows your device preference.", cut.Markup);
    }

    [Fact]
    public async Task Selecting_a_theme_saves_it_immediately()
    {
        var repository = new InMemorySettingsRepository(
            new AppSettings(themeMode: ThemeMode.System));
        Services.AddSingleton<ISettingsRepository>(repository);
        Services.AddScoped<ThemeState>();

        var cut = Render<Settings>();
        await cut.InvokeAsync(
            () => cut.Find("[data-testid='daypulse-theme-dark']").Click());

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                ThemeMode.Dark,
                Services.GetRequiredService<ThemeState>().Mode);
        });
        Assert.NotNull(repository.SavedSettings);
        Assert.Equal(ThemeMode.Dark, repository.SavedSettings?.ThemeMode);
    }

    [Fact]
    public async Task Theme_selector_stays_disabled_until_theme_state_is_initialized()
    {
        var repository = new DelayedSettingsRepository(
            new AppSettings(themeMode: ThemeMode.Dark));
        Services.AddSingleton<ISettingsRepository>(repository);
        Services.AddScoped<ThemeState>();

        var cut = Render<Settings>();
        Assert.True(IsThemeSelectorDisabled(cut));
        Assert.Contains("Loading theme preference", cut.Markup);

        repository.CompleteLoad();

        cut.WaitForAssertion(() =>
        {
            Assert.True(Services.GetRequiredService<ThemeState>().IsInitialized);
            Assert.Equal("true", cut.Find("[data-testid='daypulse-theme-dark']").GetAttribute("aria-pressed"));
            Assert.False(IsThemeSelectorDisabled(cut));
        });
    }

    [Fact]
    public async Task Theme_save_failure_rolls_back_and_is_visible()
    {
        var repository = new FailingSettingsRepository(
            new AppSettings(themeMode: ThemeMode.System));
        Services.AddSingleton<ISettingsRepository>(repository);
        Services.AddScoped<ThemeState>();

        var cut = Render<Settings>();
        cut.WaitForAssertion(() =>
            Assert.True(Services.GetRequiredService<ThemeState>().IsInitialized));
        await cut.InvokeAsync(
            () => cut.Find("[data-testid='daypulse-theme-dark']").Click());

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("The theme preference could not be saved.", cut.Markup);
            Assert.Equal(
                ThemeMode.System,
                Services.GetRequiredService<ThemeState>().Mode);
            Assert.Equal("true", cut.Find("[data-testid='daypulse-theme-system']").GetAttribute("aria-pressed"));
            Assert.Equal("false", cut.Find("[data-testid='daypulse-theme-dark']").GetAttribute("aria-pressed"));
        });
    }

    [Fact]
    public async Task Theme_initialization_failure_stops_loading_and_allows_a_retry()
    {
        var repository = new FailingOnceSettingsRepository(
            new AppSettings(themeMode: ThemeMode.Dark));
        Services.AddSingleton<ISettingsRepository>(repository);
        Services.AddScoped<ThemeState>();

        var cut = Render<Settings>();

        cut.WaitForAssertion(() =>
        {
            var state = Services.GetRequiredService<ThemeState>();
            Assert.True(state.IsInitializationFailed);
            Assert.False(state.IsInitialized);
            Assert.DoesNotContain("Loading theme preference", cut.Markup);
            Assert.Contains("The theme preference could not be loaded.", cut.Markup);
            Assert.DoesNotContain("??", cut.Markup);
            Assert.Contains("Try again", cut.Markup);
            Assert.True(IsThemeSelectorDisabled(cut));
        });

        await cut.InvokeAsync(() =>
            cut.Find("[data-testid='daypulse-theme-retry']").Click());

        cut.WaitForAssertion(() =>
        {
            var state = Services.GetRequiredService<ThemeState>();
            Assert.True(state.IsInitialized);
            Assert.False(state.IsInitializationFailed);
            Assert.Equal(ThemeMode.Dark, state.Mode);
            Assert.DoesNotContain("The theme preference could not be loaded.", cut.Markup);
            Assert.DoesNotContain("Loading theme preference", cut.Markup);
            Assert.False(IsThemeSelectorDisabled(cut));
        });
    }

    [Fact]
    public async Task Theme_initialization_error_clears_when_shared_state_recovers()
    {
        var repository = new FailingOnceSettingsRepository(
            new AppSettings(themeMode: ThemeMode.Light));
        Services.AddSingleton<ISettingsRepository>(repository);
        Services.AddScoped<ThemeState>();

        var cut = Render<Settings>();
        cut.WaitForAssertion(() =>
            Assert.True(
                Services.GetRequiredService<ThemeState>()
                    .IsInitializationFailed));

        await cut.InvokeAsync(() =>
            Services.GetRequiredService<ThemeState>()
                .InitializeAsync(Xunit.TestContext.Current.CancellationToken));

        cut.WaitForAssertion(() =>
        {
            var state = Services.GetRequiredService<ThemeState>();
            Assert.True(state.IsInitialized);
            Assert.False(state.IsInitializationFailed);
            Assert.DoesNotContain(
                "The theme preference could not be loaded.",
                cut.Markup);
            Assert.False(IsThemeSelectorDisabled(cut));
        });
    }

    [Fact]
    public async Task Settings_retry_keeps_provider_failure_visible_until_js_initialization_recovers()
    {
        var repository = new FailingTwiceSettingsRepository(
            new AppSettings(themeMode: ThemeMode.System));
        Services.AddSingleton<ISettingsRepository>(repository);
        Services.AddScoped<ThemeState>();

        var module = JSInterop.SetupModule("./Shared/ThemeProvider.razor.js");
        var initializeHandler = module
            .Setup<bool>("initialize", _ => true)
            .SetException(new InvalidOperationException("The test module failed."));
        module
            .Setup<bool>("setMode", _ => true)
            .SetResult(false);
        module
            .SetupVoid("dispose", _ => true)
            .SetVoidResult();

        Render<ThemeProvider>();
        var cut = Render<Settings>();

        cut.WaitForAssertion(() =>
        {
            Assert.True(
                Services.GetRequiredService<ThemeState>()
                    .IsInitializationFailed);
            Assert.Contains("Try again", cut.Markup);
        });

        await cut.InvokeAsync(() =>
            cut.Find("[data-testid='daypulse-theme-retry']").Click());

        cut.WaitForAssertion(() =>
        {
            var state = Services.GetRequiredService<ThemeState>();
            Assert.True(state.IsProviderInitializationFailed);
            Assert.False(state.IsProviderInitialized);
            Assert.Contains("The theme preference could not be loaded.", cut.Markup);
            Assert.Contains("Try again", cut.Markup);
        });

        initializeHandler.SetResult(false);
        await cut.InvokeAsync(() =>
            cut.Find("[data-testid='daypulse-theme-retry']").Click());

        cut.WaitForAssertion(() =>
        {
            var state = Services.GetRequiredService<ThemeState>();
            Assert.True(state.IsProviderInitialized);
            Assert.False(state.IsInitializationFailed);
            Assert.DoesNotContain(
                "The theme preference could not be loaded.",
                cut.Markup);
            Assert.False(IsThemeSelectorDisabled(cut));
        });
    }

    private sealed class InMemorySettingsRepository(AppSettings initialSettings)
        : ISettingsRepository
    {
        public AppSettings? SavedSettings { get; private set; }

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
            SavedSettings = settings;
            return Task.CompletedTask;
        }

        public Task ResetAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SavedSettings = null;
            return Task.CompletedTask;
        }
    }

    private static bool IsThemeSelectorDisabled(IRenderedComponent<Settings> cut)
    {
        var selector = cut.Find("[data-testid='daypulse-theme-selector']");
        var buttons = selector.QuerySelectorAll("button");
        Assert.Equal(3, buttons.Length);
        return buttons.All(button => button.HasAttribute("disabled"));
    }

    private sealed class DelayedSettingsRepository(AppSettings initialSettings)
        : ISettingsRepository
    {
        private readonly TaskCompletionSource<AppSettings> _loadCompletion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<AppSettings> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return _loadCompletion.Task.WaitAsync(cancellationToken);
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

        public void CompleteLoad()
        {
            _loadCompletion.TrySetResult(initialSettings);
        }
    }

    private sealed class FailingSettingsRepository(AppSettings initialSettings)
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
            return Task.FromException(
                new InvalidOperationException("The test persistence failed."));
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

    private sealed class FailingTwiceSettingsRepository(AppSettings initialSettings)
        : ISettingsRepository
    {
        private int _loadAttempts;

        public Task<AppSettings> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Interlocked.Increment(ref _loadAttempts) <= 2)
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
