using System.Text.Json;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure.Repositories;

namespace DayPulse.Tests.Repositories;

public sealed class BrowserCheckInRepositoryTests
{
    [Fact]
    public async Task CreateAndRead_PersistsAcrossRepositoryInstances()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var storage = new InMemoryClientStorage();
        var checkIn = CreateCheckIn("first", new DateOnly(2026, 10, 12));

        await new BrowserCheckInRepository(storage).CreateAsync(checkIn, cancellationToken);

        var loaded = await new BrowserCheckInRepository(storage).GetByDateAsync(
            checkIn.EntryDate,
            cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(checkIn.Id, loaded.Id);
        Assert.Equal(checkIn.Mood, loaded.Mood);
        Assert.Equal(checkIn.ActivityIds, loaded.ActivityIds);
    }

    [Fact]
    public async Task Create_WithOccupiedEntryDate_ThrowsDuplicateEntryDateException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new BrowserCheckInRepository(new InMemoryClientStorage());
        var existing = CreateCheckIn("existing", new DateOnly(2026, 10, 12));
        var duplicate = CreateCheckIn("duplicate", existing.EntryDate);

        await repository.CreateAsync(existing, cancellationToken);

        var exception = await Assert.ThrowsAsync<DuplicateEntryDateException>(
            () => repository.CreateAsync(duplicate, cancellationToken));

        Assert.Equal(existing.EntryDate, exception.EntryDate);
        Assert.Equal(existing.Id, exception.ExistingCheckInId);
    }

    [Fact]
    public async Task Create_RetriesWhenAnotherContextWinsTheConditionalWrite()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var storage = new InMemoryClientStorage
        {
            ConditionalWriteFailuresRemaining = 1,
        };
        var repository = new BrowserCheckInRepository(storage);
        var checkIn = CreateCheckIn("retry", new DateOnly(2026, 10, 12));

        await repository.CreateAsync(checkIn, cancellationToken);

        Assert.NotNull(await repository.GetByIdAsync(checkIn.Id, cancellationToken));
    }

    [Fact]
    public async Task Create_WhenAnotherContextStoresTheSameDate_ThrowsDuplicateEntryDateException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var storage = new InMemoryClientStorage();
        var repository = new BrowserCheckInRepository(storage);
        var candidate = CreateCheckIn("candidate", new DateOnly(2026, 10, 12));
        var competing = CreateCheckIn("competing", candidate.EntryDate);
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        storage.InjectedConditionalWriteKey = "daypulse.check-ins";
        storage.InjectedConditionalWriteValue = JsonSerializer.Serialize(
            new[] { CreateStorageRecord(competing) },
            serializerOptions);

        var exception = await Assert.ThrowsAsync<DuplicateEntryDateException>(
            () => repository.CreateAsync(candidate, cancellationToken));

        Assert.Equal(competing.Id, exception.ExistingCheckInId);
        Assert.Equal(candidate.EntryDate, exception.EntryDate);
    }

    [Fact]
    public async Task GetAll_WhenStoredCheckInIsMissingRequiredData_ThrowsInvalidDataException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var storage = new InMemoryClientStorage();
        await storage.SetAsync(
            "daypulse.check-ins",
            """
            [
              {
                "id": "incomplete",
                "entryDate": "2026-10-12",
                "activityIds": [],
                "createdAt": "2026-10-12T08:00:00Z",
                "updatedAt": "2026-10-12T08:00:00Z"
              }
            ]
            """,
            cancellationToken);

        var repository = new BrowserCheckInRepository(storage);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => repository.GetAllAsync(cancellationToken));
    }

    [Fact]
    public async Task Update_WithSameIdAndEntryDate_UpdatesTheExistingRecord()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new BrowserCheckInRepository(new InMemoryClientStorage());
        var createdAt = new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);
        var original = new CheckIn(
            "same-record",
            new DateOnly(2026, 10, 12),
            mood: 1,
            activityIds: ["default-work"],
            createdAt: createdAt,
            updatedAt: createdAt);
        var update = new CheckIn(
            original.Id,
            original.EntryDate,
            mood: 4,
            activityIds: ["default-reading"],
            createdAt: createdAt,
            updatedAt: createdAt);

        await repository.CreateAsync(original, cancellationToken);
        await repository.UpdateAsync(update, cancellationToken);

        var loaded = await repository.GetByIdAsync(original.Id, cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(4, loaded.Mood);
        Assert.Equal(["default-reading"], loaded.ActivityIds);
        Assert.Equal(original.CreatedAt, loaded.CreatedAt);
        Assert.True(loaded.UpdatedAt >= original.UpdatedAt);
    }

    [Fact]
    public async Task Update_ToAnotherOccupiedEntryDate_ThrowsDuplicateEntryDateException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new BrowserCheckInRepository(new InMemoryClientStorage());
        var first = CreateCheckIn("first", new DateOnly(2026, 10, 12));
        var second = CreateCheckIn("second", new DateOnly(2026, 10, 13));

        await repository.CreateAsync(first, cancellationToken);
        await repository.CreateAsync(second, cancellationToken);

        var update = new CheckIn(
            second.Id,
            first.EntryDate,
            mood: second.Mood,
            createdAt: second.CreatedAt,
            updatedAt: second.UpdatedAt);

        var exception = await Assert.ThrowsAsync<DuplicateEntryDateException>(
            () => repository.UpdateAsync(update, cancellationToken));

        Assert.Equal(first.EntryDate, exception.EntryDate);
        Assert.Equal(first.Id, exception.ExistingCheckInId);
    }

    private static CheckIn CreateCheckIn(string id, DateOnly entryDate)
    {
        var timestamp = new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);
        return new CheckIn(
            id,
            entryDate,
            mood: 2,
            energy: 7,
            activityIds: ["default-work"],
            notes: "A note",
            createdAt: timestamp,
            updatedAt: timestamp);
    }

    private static object CreateStorageRecord(CheckIn checkIn)
    {
        return new
        {
            checkIn.Id,
            checkIn.EntryDate,
            checkIn.Mood,
            checkIn.Energy,
            checkIn.Anxiety,
            checkIn.Focus,
            checkIn.SleepHours,
            checkIn.ActivityIds,
            checkIn.Notes,
            checkIn.CreatedAt,
            checkIn.UpdatedAt,
        };
    }
}
