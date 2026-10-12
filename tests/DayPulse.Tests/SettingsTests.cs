using Bunit;
using DayPulse.App.Pages;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class SettingsTests : BunitContext
{
    public SettingsTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
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
        var selector = cut.FindComponent<MudSelect<ThemeMode>>();

        Assert.Contains("Theme preference", cut.Markup);
        Assert.Equal(
            ThemeMode.Dark,
            selector.Instance.GetState(themeSelector => themeSelector.Value));
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
        var selector = cut.FindComponent<MudSelect<ThemeMode>>();

        await cut.InvokeAsync(
            () => selector.Instance.ValueChanged.InvokeAsync(ThemeMode.Dark));

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
        var selector = cut.FindComponent<MudSelect<ThemeMode>>();

        Assert.True(IsThemeSelectorDisabled(cut));
        Assert.Contains("Loading theme preference", cut.Markup);

        repository.CompleteLoad();

        cut.WaitForAssertion(() =>
        {
            Assert.True(Services.GetRequiredService<ThemeState>().IsInitialized);
            Assert.Equal(
                ThemeMode.Dark,
                selector.Instance.GetState(themeSelector => themeSelector.Value));
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
        var selector = cut.FindComponent<MudSelect<ThemeMode>>();

        await cut.InvokeAsync(
            () => selector.Instance.ValueChanged.InvokeAsync(ThemeMode.Dark));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("The theme preference could not be saved.", cut.Markup);
            Assert.Equal(
                ThemeMode.System,
                Services.GetRequiredService<ThemeState>().Mode);
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
        return selector.HasAttribute("disabled")
            || selector.QuerySelector("[disabled]") is not null;
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
