using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure.Repositories;

namespace DayPulse.Tests.Repositories;

public sealed class BrowserActivityRepositoryTests
{
    private static readonly string[] BuiltInIds =
    [
        "default-work",
        "default-exercise",
        "default-walking",
        "default-reading",
        "default-gaming",
        "default-family",
        "default-friends",
        "default-travel",
        "default-relaxing",
        "default-housework",
        "default-cooking",
        "default-shopping",
        "default-movies",
        "default-music",
        "default-study",
        "default-meditation",
    ];

    [Fact]
    public async Task SeedDefaults_CreatesExactlySixteenStableBuiltIns()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new BrowserActivityRepository(new InMemoryClientStorage());

        await repository.SeedDefaultsAsync(cancellationToken);

        var activities = await repository.GetAllAsync(cancellationToken);

        Assert.Equal(16, activities.Count);
        Assert.All(activities, activity => Assert.True(activity.IsDefault));
        Assert.Equal(BuiltInIds, activities.Select(activity => activity.Id));
        Assert.Equal(
            ["Work", "Exercise", "Walking", "Reading", "Gaming", "Family", "Friends",
             "Travel", "Relaxing", "Housework", "Cooking", "Shopping", "Movies", "Music",
             "Study", "Meditation"],
            activities.Select(activity => activity.Name));
    }

    [Fact]
    public async Task SeedDefaults_IsIdempotentAndDoesNotRewriteExistingBuiltIns()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var storage = new InMemoryClientStorage();
        var repository = new BrowserActivityRepository(storage);

        await repository.SeedDefaultsAsync(cancellationToken);
        var writesAfterFirstSeed = storage.SetCount;
        var firstSeed = await repository.GetAllAsync(cancellationToken);

        await repository.SeedDefaultsAsync(cancellationToken);

        var secondSeed = await repository.GetAllAsync(cancellationToken);
        Assert.Equal(writesAfterFirstSeed, storage.SetCount);
        Assert.Equal(firstSeed.Select(activity => activity.Id), secondSeed.Select(activity => activity.Id));
        Assert.Equal(firstSeed.Select(activity => activity.CreatedAt), secondSeed.Select(activity => activity.CreatedAt));
    }

    [Fact]
    public async Task Delete_BuiltInActivity_ThrowsAndLeavesTheActivityStored()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new BrowserActivityRepository(new InMemoryClientStorage());

        await repository.SeedDefaultsAsync(cancellationToken);

        var exception = await Assert.ThrowsAsync<BuiltInActivityDeleteException>(
            () => repository.DeleteAsync("default-work", cancellationToken));

        Assert.Equal("default-work", exception.ActivityId);
        Assert.NotNull(await repository.GetByIdAsync("default-work", cancellationToken));
    }

    [Fact]
    public async Task Deactivate_BuiltInActivity_SoftDeletesItWithoutReseeding()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new BrowserActivityRepository(new InMemoryClientStorage());

        await repository.SetActiveAsync("default-reading", false, cancellationToken);

        var inactive = await repository.GetByIdAsync("default-reading", cancellationToken);
        var active = await repository.GetActiveAsync(cancellationToken);

        Assert.NotNull(inactive);
        Assert.False(inactive.IsActive);
        Assert.DoesNotContain(active, activity => activity.Id == "default-reading");

        await repository.SeedDefaultsAsync(cancellationToken);

        inactive = await repository.GetByIdAsync("default-reading", cancellationToken);
        Assert.NotNull(inactive);
        Assert.False(inactive.IsActive);
    }

    [Fact]
    public async Task Update_BuiltInActivityNameOrSortOrder_ThrowsAndPreservesCatalogMetadata()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new BrowserActivityRepository(new InMemoryClientStorage());

        var original = await repository.GetByIdAsync("default-work", cancellationToken);
        Assert.NotNull(original);

        var renamed = new Activity(
            original.Id,
            "Renamed",
            isDefault: true,
            isActive: false,
            sortOrder: original.SortOrder,
            createdAt: original.CreatedAt,
            updatedAt: original.UpdatedAt);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.UpdateAsync(renamed, cancellationToken));

        var stored = await repository.GetByIdAsync("default-work", cancellationToken);
        Assert.NotNull(stored);
        Assert.Equal(original.Name, stored.Name);
        Assert.Equal(original.SortOrder, stored.SortOrder);
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task AddCustomActivity_ThenDelete_RemovesOnlyTheCustomActivity()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new BrowserActivityRepository(new InMemoryClientStorage());
        var custom = new Activity("custom", "Custom", sortOrder: 100);

        await repository.AddAsync(custom, cancellationToken);
        await repository.DeleteAsync(custom.Id, cancellationToken);

        Assert.Null(await repository.GetByIdAsync(custom.Id, cancellationToken));
        Assert.Equal(16, (await repository.GetAllAsync(cancellationToken)).Count);
    }
}
