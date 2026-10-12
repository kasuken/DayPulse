using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure.Storage;

namespace DayPulse.Infrastructure.Repositories;

/// <summary>
/// Persists activities in browser local storage and owns the built-in catalog seed.
/// </summary>
public sealed class BrowserActivityRepository : IActivityRepository
{
    private readonly IAtomicClientStorage _storage;

    // This gate avoids redundant in-process races. The conditional browser write also
    // protects read-modify-write operations across browser contexts.
    private static readonly SemaphoreSlim StorageGate = new(1, 1);

    /// <summary>
    /// Creates a browser-backed activity repository.
    /// </summary>
    /// <param name="storage">The browser storage implementation.</param>
    public BrowserActivityRepository(IAtomicClientStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Activity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            var snapshot = await ReadSeededSnapshotAsync(cancellationToken);
            return Order(snapshot.Activities);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Activity>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            var snapshot = await ReadSeededSnapshotAsync(cancellationToken);
            return Order(snapshot.Activities.Where(activity => activity.IsActive));
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Activity?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            var snapshot = await ReadSeededSnapshotAsync(cancellationToken);
            return snapshot.Activities.FirstOrDefault(activity =>
                string.Equals(activity.Id, id, StringComparison.Ordinal));
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task AddAsync(
        Activity activity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activity);

        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= RepositoryJson.MaxMutationAttempts; attempt++)
            {
                var snapshot = await ReadSeededSnapshotAsync(cancellationToken);
                var activities = snapshot.Activities;

                if (activities.Any(existing =>
                        string.Equals(existing.Id, activity.Id, StringComparison.Ordinal)))
                {
                    throw new InvalidOperationException(
                        $"An activity with id '{activity.Id}' already exists.");
                }

                if (activity.IsDefault || BuiltInActivityCatalog.IsBuiltInId(activity.Id))
                {
                    throw new InvalidOperationException(
                        "Only the built-in activity catalog can create built-in activities.");
                }

                activities.Add(activity);
                if (await TryCommitAsync(
                        snapshot.Raw,
                        activities,
                        cancellationToken))
                {
                    return;
                }
            }

            throw new RepositoryConcurrencyException(
                RepositoryStorageKeys.Activities,
                RepositoryJson.MaxMutationAttempts);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(
        Activity activity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activity);

        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= RepositoryJson.MaxMutationAttempts; attempt++)
            {
                var snapshot = await ReadSeededSnapshotAsync(cancellationToken);
                var activities = snapshot.Activities;
                var index = activities.FindIndex(existing =>
                    string.Equals(existing.Id, activity.Id, StringComparison.Ordinal));
                if (index < 0)
                {
                    throw new RepositoryEntityNotFoundException(
                        nameof(Activity),
                        activity.Id);
                }

                var existing = activities[index];
                if (existing.IsDefault != activity.IsDefault)
                {
                    throw new InvalidOperationException(
                        "An activity's built-in status cannot be changed.");
                }

                if (existing.IsDefault
                    && (!string.Equals(existing.Name, activity.Name, StringComparison.Ordinal)
                        || existing.SortOrder != activity.SortOrder))
                {
                    throw new InvalidOperationException(
                        "Built-in activity names and sort order cannot be changed.");
                }

                activities[index] = new Activity(
                    activity.Id,
                    activity.Name,
                    activity.IsDefault,
                    activity.IsActive,
                    activity.SortOrder,
                    createdAt: existing.CreatedAt,
                    updatedAt: DateTimeOffset.UtcNow);

                if (await TryCommitAsync(
                        snapshot.Raw,
                        activities,
                        cancellationToken))
                {
                    return;
                }
            }

            throw new RepositoryConcurrencyException(
                RepositoryStorageKeys.Activities,
                RepositoryJson.MaxMutationAttempts);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task SetActiveAsync(
        string id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= RepositoryJson.MaxMutationAttempts; attempt++)
            {
                var snapshot = await ReadSeededSnapshotAsync(cancellationToken);
                var activities = snapshot.Activities;
                var index = activities.FindIndex(activity =>
                    string.Equals(activity.Id, id, StringComparison.Ordinal));
                if (index < 0)
                {
                    throw new RepositoryEntityNotFoundException(nameof(Activity), id);
                }

                var existing = activities[index];
                if (existing.IsActive == isActive)
                {
                    return;
                }

                activities[index] = new Activity(
                    existing.Id,
                    existing.Name,
                    existing.IsDefault,
                    isActive,
                    existing.SortOrder,
                    createdAt: existing.CreatedAt,
                    updatedAt: DateTimeOffset.UtcNow);

                if (await TryCommitAsync(
                        snapshot.Raw,
                        activities,
                        cancellationToken))
                {
                    return;
                }
            }

            throw new RepositoryConcurrencyException(
                RepositoryStorageKeys.Activities,
                RepositoryJson.MaxMutationAttempts);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public Task DeactivateAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return SetActiveAsync(id, false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= RepositoryJson.MaxMutationAttempts; attempt++)
            {
                var snapshot = await ReadSeededSnapshotAsync(cancellationToken);
                var activities = snapshot.Activities;
                var activity = activities.FirstOrDefault(existing =>
                    string.Equals(existing.Id, id, StringComparison.Ordinal));
                if (activity is null)
                {
                    return;
                }

                if (activity.IsDefault)
                {
                    throw new BuiltInActivityDeleteException(id);
                }

                activities.Remove(activity);
                if (await TryCommitAsync(
                        snapshot.Raw,
                        activities,
                        cancellationToken))
                {
                    return;
                }
            }

            throw new RepositoryConcurrencyException(
                RepositoryStorageKeys.Activities,
                RepositoryJson.MaxMutationAttempts);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task SeedDefaultsAsync(CancellationToken cancellationToken = default)
    {
        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            await ReadSeededSnapshotAsync(cancellationToken);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    private async Task<(string? Raw, List<Activity> Activities)> ReadSeededSnapshotAsync(
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= RepositoryJson.MaxMutationAttempts; attempt++)
        {
            var snapshot = await RepositoryJson.ReadListSnapshotAsync<ActivityStorageModel>(
                _storage,
                RepositoryStorageKeys.Activities,
                cancellationToken);
            var activities = snapshot.Value
                .Select(activity => activity is null
                    ? throw new InvalidDataException(
                        "Stored activities contain a null record.")
                    : activity.ToDomain())
                .ToList();
            EnsureUniqueIds(activities);

            var defaults = BuiltInActivityCatalog.CreateDefaults(DateTimeOffset.UtcNow);
            if (defaults.Count != BuiltInActivityCatalog.Count)
            {
                throw new InvalidDataException(
                    "The built-in activity catalog does not contain the expected number of activities.");
            }

            var defaultsById = defaults.ToDictionary(
                activity => activity.Id,
                StringComparer.Ordinal);
            var existingById = activities.ToDictionary(
                activity => activity.Id,
                StringComparer.Ordinal);

            foreach (var activity in activities)
            {
                if (!BuiltInActivityCatalog.IsBuiltInId(activity.Id))
                {
                    continue;
                }

                if (!activity.IsDefault)
                {
                    throw new InvalidDataException(
                        $"Stored built-in activity '{activity.Id}' is not marked as built-in.");
                }

                var defaultActivity = defaultsById[activity.Id];
                if (!string.Equals(activity.Name, defaultActivity.Name, StringComparison.Ordinal)
                    || activity.SortOrder != defaultActivity.SortOrder)
                {
                    throw new InvalidDataException(
                        $"Stored built-in activity '{activity.Id}' has immutable catalog metadata changes.");
                }
            }

            var missingDefaults = defaults
                .Where(activity => !existingById.ContainsKey(activity.Id))
                .ToArray();

            if (missingDefaults.Length == 0)
            {
                return (snapshot.Raw, activities);
            }

            activities.AddRange(missingDefaults);
            var updatedRaw = Serialize(activities);
            if (await _storage.TrySetAsync(
                    RepositoryStorageKeys.Activities,
                    snapshot.Raw,
                    updatedRaw,
                    cancellationToken))
            {
                return (updatedRaw, activities);
            }
        }

        throw new RepositoryConcurrencyException(
            RepositoryStorageKeys.Activities,
            RepositoryJson.MaxMutationAttempts);
    }

    private ValueTask<bool> TryCommitAsync(
        string? expectedRaw,
        IReadOnlyCollection<Activity> activities,
        CancellationToken cancellationToken)
    {
        return _storage.TrySetAsync(
            RepositoryStorageKeys.Activities,
            expectedRaw,
            Serialize(activities),
            cancellationToken);
    }

    private static string Serialize(IEnumerable<Activity> activities)
    {
        var values = Order(activities)
            .Select(ActivityStorageModel.FromDomain)
            .ToArray();
        return RepositoryJson.Serialize(values);
    }

    private static void EnsureUniqueIds(IReadOnlyList<Activity> activities)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var activity in activities)
        {
            if (!ids.Add(activity.Id))
            {
                throw new InvalidDataException(
                    $"Stored activities contain duplicate id '{activity.Id}'.");
            }
        }
    }

    private static IReadOnlyList<Activity> Order(IEnumerable<Activity> activities)
    {
        return activities
            .OrderBy(activity => activity.SortOrder)
            .ThenBy(activity => activity.Name, StringComparer.Ordinal)
            .ThenBy(activity => activity.Id, StringComparer.Ordinal)
            .ToArray();
    }
}
