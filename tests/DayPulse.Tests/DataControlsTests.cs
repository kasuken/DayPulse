using System.Text.Json;
using Bunit;
using Bunit.TestDoubles;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure.Repositories;
using DayPulse.Infrastructure.Storage;
using DayPulse.Tests.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class DataControlsTests : BunitContext
{
    private static CancellationToken TestCancellation => Xunit.TestContext.Current.CancellationToken;

    private readonly FailingWriteStorage _storage = new();
    private readonly BrowserCheckInRepository _checkIns;
    private readonly BrowserActivityRepository _activities;
    private readonly BrowserSettingsRepository _settings;
    private readonly BunitJSModuleInterop _module;

    public DataControlsTests()
    {
        Services.AddMudServices();
        Services.AddDayPulseLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
        _module = JSInterop.SetupModule("./Shared/DataControls.razor.js");
        _module.SetupVoid("download", _ => true).SetVoidResult();
        _module.SetupVoid("resetInstallDismissal", _ => true).SetVoidResult();
        _checkIns = new BrowserCheckInRepository(_storage);
        _activities = new BrowserActivityRepository(_storage);
        _settings = new BrowserSettingsRepository(_storage);
        Services.AddSingleton<ICheckInRepository>(_checkIns);
        Services.AddSingleton<IActivityRepository>(_activities);
        Services.AddSingleton<ISettingsRepository>(_settings);
    }

    [Fact]
    public async Task Delete_requires_explicit_confirmation_and_cancel_leaves_data_intact()
    {
        await SeedAsync();
        var cut = Render<DataControls>();

        await cut.Find("[data-testid='data-request-delete']").ClickAsync(new());

        Assert.True(cut.Find("[data-testid='data-confirm-delete']").HasAttribute("disabled"));
        Assert.Single(await _checkIns.GetAllAsync(TestCancellation));
        await cut.Find("[data-testid='data-cancel-delete']").ClickAsync(new());
        Assert.Single(await _checkIns.GetAllAsync(TestCancellation));
        Assert.NotNull(await _activities.GetByIdAsync("custom", TestCancellation));
        Assert.True((await _settings.LoadAsync(TestCancellation)).OnboardingCompleted);
    }

    [Fact]
    public async Task Confirmed_delete_removes_checkins_and_custom_activities_resets_settings_and_reloads()
    {
        await SeedAsync();
        var cut = Render<DataControls>();

        await ConfirmDeleteAsync(cut);

        Assert.Empty(await _checkIns.GetAllAsync(TestCancellation));
        Assert.Null(await _activities.GetByIdAsync("custom", TestCancellation));
        Assert.NotEmpty(await _activities.GetAllAsync(TestCancellation));
        Assert.All(await _activities.GetAllAsync(TestCancellation), activity => Assert.True(activity.IsDefault));
        Assert.False((await _settings.LoadAsync(TestCancellation)).OnboardingCompleted);
        var navigation = Assert.IsType<BunitNavigationManager>(Services.GetRequiredService<NavigationManager>());
        var reload = Assert.Single(navigation.History);
        Assert.Equal(navigation.BaseUri, reload.Uri);
        Assert.True(reload.Options.ForceLoad);
        Assert.Single(_module.Invocations, invocation => invocation.Identifier == "resetInstallDismissal");
        Assert.Empty(cut.FindAll("[data-testid='data-error']"));
    }

    [Fact]
    public async Task Partial_delete_failure_never_claims_success_or_resets_settings()
    {
        await SeedAsync();
        _storage.FailingKey = "daypulse.check-ins";
        var cut = Render<DataControls>();

        await ConfirmDeleteAsync(cut);

        Assert.Single(await _checkIns.GetAllAsync(TestCancellation));
        Assert.Null(await _activities.GetByIdAsync("custom", TestCancellation));
        Assert.True((await _settings.LoadAsync(TestCancellation)).OnboardingCompleted);
        Assert.Contains("Deletion did not fully complete", cut.Find("[data-testid='data-error']").TextContent);
        Assert.DoesNotContain("Download requested", cut.Markup);
        Assert.DoesNotContain(_module.Invocations, invocation => invocation.Identifier == "resetInstallDismissal");
        Assert.Empty(Assert.IsType<BunitNavigationManager>(Services.GetRequiredService<NavigationManager>()).History);
    }

    [Fact]
    public async Task Settings_reset_failure_reports_partial_deletion_and_does_not_reload()
    {
        await SeedAsync();
        _storage.FailingKey = "daypulse.settings";
        var cut = Render<DataControls>();

        await ConfirmDeleteAsync(cut);

        Assert.Empty(await _checkIns.GetAllAsync(TestCancellation));
        Assert.Null(await _activities.GetByIdAsync("custom", TestCancellation));
        Assert.True((await _settings.LoadAsync(TestCancellation)).OnboardingCompleted);
        Assert.Contains("Deletion did not fully complete", cut.Find("[data-testid='data-error']").TextContent);
        Assert.Empty(Assert.IsType<BunitNavigationManager>(Services.GetRequiredService<NavigationManager>()).History);
    }

    [Fact]
    public async Task A_failed_export_never_starts_a_download_or_deletes_data()
    {
        await SeedAsync();
        _storage.FailingReadKey = "daypulse.check-ins";
        var cut = Render<DataControls>();

        await cut.Find("[data-testid='data-export-json']").ClickAsync(new());

        Assert.Contains("The export could not be created or downloaded", cut.Find("[data-testid='data-error']").TextContent);
        Assert.DoesNotContain(_module.Invocations, invocation => invocation.Identifier == "download");
        _storage.FailingReadKey = null;
        Assert.Single(await _checkIns.GetAllAsync(TestCancellation));
    }

    [Fact]
    public async Task Json_download_uses_real_repository_data_without_uploading()
    {
        await SeedAsync();
        var cut = Render<DataControls>();

        await cut.Find("[data-testid='data-export-json']").ClickAsync(new());

        var invocation = Assert.Single(_module.Invocations, invocation => invocation.Identifier == "download");
        Assert.Equal("daypulse.json", invocation.Arguments[0]);
        using var export = JsonDocument.Parse(Assert.IsType<string>(invocation.Arguments[2]));
        Assert.Equal("Personal note", export.RootElement.GetProperty("CheckIns")[0].GetProperty("Notes").GetString());
        Assert.Contains(export.RootElement.GetProperty("Activities").EnumerateArray(),
            activity => activity.GetProperty("Id").GetString() == "custom");
        Assert.True(export.RootElement.GetProperty("Settings").GetProperty("OnboardingCompleted").GetBoolean());
        Assert.Single(await _checkIns.GetAllAsync(TestCancellation));
        Assert.Contains("Download requested", cut.Markup);
    }

    private async Task SeedAsync()
    {
        await _checkIns.CreateAsync(new CheckIn("entry", new DateOnly(2026, 11, 3), 2,
            activityIds: ["custom"], notes: "Personal note"), TestCancellation);
        await _activities.AddAsync(new Activity("custom", "Private activity"), TestCancellation);
        await _settings.SaveAsync(new AppSettings(onboardingCompleted: true, themeMode: ThemeMode.Dark), TestCancellation);
    }

    private static async Task ConfirmDeleteAsync(IRenderedComponent<DataControls> cut)
    {
        await cut.Find("[data-testid='data-request-delete']").ClickAsync(new());
        await cut.Find("input[type='checkbox']").ChangeAsync(new ChangeEventArgs { Value = true });
        await cut.Find("[data-testid='data-confirm-delete']").ClickAsync(new());
    }

    private sealed class FailingWriteStorage : IAtomicClientStorage
    {
        private readonly InMemoryClientStorage _inner = new();
        public string? FailingKey { get; set; }
        public string? FailingReadKey { get; set; }

        public ValueTask<string?> GetAsync(string key, CancellationToken cancellationToken = default) =>
            key == FailingReadKey
                ? ValueTask.FromException<string?>(new InvalidOperationException("Storage read failed."))
                : _inner.GetAsync(key, cancellationToken);

        public ValueTask SetAsync(string key, string value, CancellationToken cancellationToken = default) =>
            _inner.SetAsync(key, value, cancellationToken);

        public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default) =>
            _inner.RemoveAsync(key, cancellationToken);

        public ValueTask<bool> TrySetAsync(string key, string? expectedValue, string value, CancellationToken cancellationToken = default) =>
            key == FailingKey
                ? ValueTask.FromException<bool>(new InvalidOperationException("Storage write failed."))
                : _inner.TrySetAsync(key, expectedValue, value, cancellationToken);

        public ValueTask<bool> TryRemoveAsync(string key, string? expectedValue, CancellationToken cancellationToken = default) =>
            key == FailingKey
                ? ValueTask.FromException<bool>(new InvalidOperationException("Storage removal failed."))
                : _inner.TryRemoveAsync(key, expectedValue, cancellationToken);
    }
}
