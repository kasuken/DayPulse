using Bunit;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure;
using DayPulse.Infrastructure.Repositories;
using DayPulse.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace DayPulse.Tests;

public sealed class InfrastructureRegistrationTests
{
    [Fact]
    public void AddInfrastructure_registers_browser_storage_services()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure();

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IClientStorage)
                && descriptor.ImplementationType == typeof(BrowserStorage));
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IIndexedDbStorage)
                && descriptor.ImplementationType == typeof(IndexedDbStorage));
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(ICheckInRepository)
                && descriptor.ImplementationType == typeof(BrowserCheckInRepository));
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IActivityRepository)
                && descriptor.ImplementationType == typeof(BrowserActivityRepository));
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(ISettingsRepository)
                && descriptor.ImplementationType == typeof(BrowserSettingsRepository));
    }

    [Fact]
    public async Task IndexedDbStorage_imports_the_app_storage_module()
    {
        await using var context = new BunitContext();
        var module = context.JSInterop.SetupModule("./js/daypulse-storage.js");
        module.Setup<string?>("indexedDbGet", _ => true).SetResult("value");
        await using var storage = new IndexedDbStorage(context.JSInterop.JSRuntime);

        var value = await storage.GetAsync(
            "daypulse",
            "records",
            "today",
            Xunit.TestContext.Current.CancellationToken);

        Assert.Equal("value", value);
        context.JSInterop.VerifyInvoke("import");
    }

    [Fact]
    public async Task BrowserStorage_conditional_write_uses_the_app_storage_module()
    {
        await using var context = new BunitContext();
        var module = context.JSInterop.SetupModule("./js/daypulse-storage.js");
        module.Setup<bool>("localStorageTrySet", _ => true).SetResult(true);
        await using var storage = new BrowserStorage(context.JSInterop.JSRuntime);

        var succeeded = await storage.TrySetAsync(
            "daypulse.test",
            expectedValue: null,
            value: "value",
            Xunit.TestContext.Current.CancellationToken);

        Assert.True(succeeded);
        context.JSInterop.VerifyInvoke("import");
    }

    [Fact]
    public async Task BrowserStorage_conditional_remove_uses_the_app_storage_module()
    {
        await using var context = new BunitContext();
        var module = context.JSInterop.SetupModule("./js/daypulse-storage.js");
        module.Setup<bool>("localStorageTryRemove", _ => true).SetResult(true);
        await using var storage = new BrowserStorage(context.JSInterop.JSRuntime);

        var succeeded = await storage.TryRemoveAsync(
            "daypulse.test",
            expectedValue: "value",
            Xunit.TestContext.Current.CancellationToken);

        Assert.True(succeeded);
        context.JSInterop.VerifyInvoke("import");
    }
}
