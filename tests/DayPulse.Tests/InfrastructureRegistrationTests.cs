using Bunit;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure;
using DayPulse.Infrastructure.Repositories;
using DayPulse.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace DayPulse.Tests;

public sealed class InfrastructureRegistrationTests
{
    private const string StorageModulePath = "./js/daypulse-storage.js";

    [Fact]
    public void AddInfrastructure_registers_indexeddb_storage_and_repositories()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure();

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IAtomicClientStorage)
                && descriptor.ImplementationType == typeof(IndexedDbClientStorage));
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IClientStorage));
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
    public async Task Storage_services_share_one_instance_per_scope()
    {
        await using var context = new BunitContext();
        await using var provider = new ServiceCollection()
            .AddSingleton(context.JSInterop.JSRuntime)
            .AddInfrastructure()
            .BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        Assert.Same(
            scope.ServiceProvider.GetRequiredService<IAtomicClientStorage>(),
            scope.ServiceProvider.GetRequiredService<IClientStorage>());
    }

    [Fact]
    public async Task IndexedDbClientStorage_reads_through_the_app_storage_module()
    {
        await using var context = new BunitContext();
        var module = context.JSInterop.SetupModule(StorageModulePath);
        module.Setup<string?>("storageGet", _ => true).SetResult("value");
        await using var storage = new IndexedDbClientStorage(context.JSInterop.JSRuntime);

        var value = await storage.GetAsync(
            "daypulse.check-ins",
            Xunit.TestContext.Current.CancellationToken);

        Assert.Equal("value", value);
        context.JSInterop.VerifyInvoke("import");
        Assert.Equal("daypulse.check-ins", Assert.Single(module.Invocations).Arguments[0]);
    }

    [Fact]
    public async Task IndexedDbClientStorage_writes_and_removes_through_the_app_storage_module()
    {
        await using var context = new BunitContext();
        var module = context.JSInterop.SetupModule(StorageModulePath);
        module.SetupVoid("storageSet", _ => true).SetVoidResult();
        module.SetupVoid("storageRemove", _ => true).SetVoidResult();
        await using var storage = new IndexedDbClientStorage(context.JSInterop.JSRuntime);

        await storage.SetAsync("daypulse.settings", "{}", Xunit.TestContext.Current.CancellationToken);
        await storage.RemoveAsync("daypulse.settings", Xunit.TestContext.Current.CancellationToken);

        Assert.Collection(
            module.Invocations,
            set => Assert.Equal("storageSet", set.Identifier),
            remove => Assert.Equal("storageRemove", remove.Identifier));
    }

    [Fact]
    public async Task IndexedDbClientStorage_conditional_write_uses_the_app_storage_module()
    {
        await using var context = new BunitContext();
        var module = context.JSInterop.SetupModule(StorageModulePath);
        module.Setup<bool>("storageTrySet", _ => true).SetResult(true);
        await using var storage = new IndexedDbClientStorage(context.JSInterop.JSRuntime);

        var succeeded = await storage.TrySetAsync(
            "daypulse.test",
            expectedValue: null,
            value: "value",
            Xunit.TestContext.Current.CancellationToken);

        Assert.True(succeeded);
        var invocation = Assert.Single(module.Invocations);
        Assert.Equal(new object?[] { "daypulse.test", null, "value" }, invocation.Arguments);
    }

    [Fact]
    public async Task IndexedDbClientStorage_conditional_remove_uses_the_app_storage_module()
    {
        await using var context = new BunitContext();
        var module = context.JSInterop.SetupModule(StorageModulePath);
        module.Setup<bool>("storageTryRemove", _ => true).SetResult(true);
        await using var storage = new IndexedDbClientStorage(context.JSInterop.JSRuntime);

        var succeeded = await storage.TryRemoveAsync(
            "daypulse.test",
            expectedValue: "value",
            Xunit.TestContext.Current.CancellationToken);

        Assert.True(succeeded);
        var invocation = Assert.Single(module.Invocations);
        Assert.Equal(new object?[] { "daypulse.test", "value" }, invocation.Arguments);
    }

    [Fact]
    public async Task IndexedDbClientStorage_rejects_use_after_disposal()
    {
        await using var context = new BunitContext();
        context.JSInterop.SetupModule(StorageModulePath);
        var storage = new IndexedDbClientStorage(context.JSInterop.JSRuntime);
        await storage.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await storage.GetAsync("daypulse.test", Xunit.TestContext.Current.CancellationToken));
    }
}
