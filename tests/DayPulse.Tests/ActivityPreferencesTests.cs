using Bunit;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure.Repositories;
using DayPulse.Tests.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class ActivityPreferencesTests : BunitContext
{
    private static CancellationToken TestCancellation => Xunit.TestContext.Current.CancellationToken;

    public ActivityPreferencesTests()
    {
        Services.AddMudServices();
        Services.AddDayPulseLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Renaming_and_deactivating_preserves_identifiers_and_old_checkins()
    {
        var storage = new InMemoryClientStorage();
        var repository = new BrowserActivityRepository(storage);
        var checkIns = new BrowserCheckInRepository(storage);
        await repository.AddAsync(new Activity("custom", "Running"), TestCancellation);
        await checkIns.CreateAsync(new CheckIn("past", new DateOnly(2026, 11, 2), 3, activityIds: ["custom"]), TestCancellation);
        Services.AddSingleton<IActivityRepository>(repository);
        var cut = Render<ActivityPreferences>();
        var activity = cut.Find("[data-activity-id='custom']");

        var nameInput = activity.QuerySelector("input[type='text']");
        Assert.NotNull(nameInput);
        await nameInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "Jogging" });
        await cut.Find("[data-activity-id='custom'] [data-action='rename']").ClickAsync(new());
        await cut.Find("[data-activity-id='custom'] input[type='checkbox']")
            .ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = false });

        var updated = await repository.GetByIdAsync("custom", TestCancellation);
        Assert.NotNull(updated);
        Assert.Equal("Jogging", updated.Name);
        Assert.False(updated.IsActive);
        Assert.Equal("custom", Assert.Single((await checkIns.GetAllAsync(TestCancellation))[0].ActivityIds));
    }

    [Fact]
    public async Task Creating_and_reordering_an_activity_persists_the_custom_catalog()
    {
        var repository = new BrowserActivityRepository(new InMemoryClientStorage());
        var defaults = await repository.GetAllAsync(TestCancellation);
        await repository.AddAsync(new Activity("existing", "Gardening",
            sortOrder: defaults.Max(activity => activity.SortOrder) + 1), TestCancellation);
        Services.AddSingleton<IActivityRepository>(repository);
        var cut = Render<ActivityPreferences>();
        var initial = await repository.GetAllAsync(TestCancellation);

        await cut.Find("input[type='text']")
            .InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "Painting" });
        await cut.Find("[data-testid='activities-add']").ClickAsync(new());
        var created = Assert.Single(await repository.GetAllAsync(TestCancellation), activity => activity.Name == "Painting");
        await cut.Find($"[data-activity-id='{created.Id}'] [data-action='up']").ClickAsync(new());

        var catalog = await repository.GetAllAsync(TestCancellation);
        Assert.Equal("Painting", catalog[^2].Name);
        Assert.Equal(created.Id, catalog[^2].Id);
        Assert.Equal(initial[^1].Id, catalog[^1].Id);
        Assert.Equal(initial.Count + 1, catalog.Count);
    }
}
