using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure.Storage;

namespace DayPulse.Infrastructure.Repositories;

/// <summary>
/// Persists check-ins in browser IndexedDB storage.
/// </summary>
public sealed class BrowserCheckInRepository : ICheckInRepository
{
    private readonly IAtomicClientStorage _storage;

    // This gate avoids redundant in-process races. The conditional browser write also
    // protects read-modify-write operations across browser contexts.
    private static readonly SemaphoreSlim StorageGate = new(1, 1);

    /// <summary>
    /// Creates a browser-backed check-in repository.
    /// </summary>
    /// <param name="storage">The browser storage implementation.</param>
    public BrowserCheckInRepository(IAtomicClientStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CheckIn>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            var checkIns = await ReadAsync(cancellationToken);
            return Order(checkIns);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CheckIn>> GetBetweenAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException(
                "The start date must not be after the end date.",
                nameof(startDate));
        }

        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            var checkIns = await ReadAsync(cancellationToken);
            return Order(
                checkIns.Where(checkIn =>
                    checkIn.EntryDate >= startDate
                    && checkIn.EntryDate <= endDate));
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<CheckIn?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            var checkIns = await ReadAsync(cancellationToken);
            return checkIns.FirstOrDefault(checkIn =>
                string.Equals(checkIn.Id, id, StringComparison.Ordinal));
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<CheckIn?> GetByDateAsync(
        DateOnly entryDate,
        CancellationToken cancellationToken = default)
    {
        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            var checkIns = await ReadAsync(cancellationToken);
            return checkIns.FirstOrDefault(checkIn => checkIn.EntryDate == entryDate);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task CreateAsync(
        CheckIn checkIn,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkIn);

        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= RepositoryJson.MaxMutationAttempts; attempt++)
            {
                var snapshot = await ReadSnapshotAsync(cancellationToken);
                var checkIns = snapshot.CheckIns;

                if (checkIns.Any(existing =>
                        string.Equals(existing.Id, checkIn.Id, StringComparison.Ordinal)))
                {
                    throw new InvalidOperationException(
                        $"A check-in with id '{checkIn.Id}' already exists.");
                }

                ThrowIfDateIsOccupied(checkIns, checkIn);
                checkIns.Add(checkIn);

                if (await TryCommitAsync(
                        snapshot.Raw,
                        checkIns,
                        cancellationToken))
                {
                    return;
                }
            }

            throw new RepositoryConcurrencyException(
                RepositoryStorageKeys.CheckIns,
                RepositoryJson.MaxMutationAttempts);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(
        CheckIn checkIn,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkIn);

        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= RepositoryJson.MaxMutationAttempts; attempt++)
            {
                var snapshot = await ReadSnapshotAsync(cancellationToken);
                var checkIns = snapshot.CheckIns;
                var index = checkIns.FindIndex(existing =>
                    string.Equals(existing.Id, checkIn.Id, StringComparison.Ordinal));
                if (index < 0)
                {
                    throw new RepositoryEntityNotFoundException(
                        nameof(CheckIn),
                        checkIn.Id);
                }

                ThrowIfDateIsOccupied(checkIns, checkIn, checkIn.Id);

                var existing = checkIns[index];
                checkIns[index] = new CheckIn(
                    checkIn.Id,
                    checkIn.EntryDate,
                    checkIn.Mood,
                    checkIn.Energy,
                    checkIn.Anxiety,
                    checkIn.Focus,
                    checkIn.SleepHours,
                    checkIn.ActivityIds,
                    checkIn.Notes,
                    createdAt: existing.CreatedAt,
                    updatedAt: DateTimeOffset.UtcNow);

                if (await TryCommitAsync(
                        snapshot.Raw,
                        checkIns,
                        cancellationToken))
                {
                    return;
                }
            }

            throw new RepositoryConcurrencyException(
                RepositoryStorageKeys.CheckIns,
                RepositoryJson.MaxMutationAttempts);
        }
        finally
        {
            StorageGate.Release();
        }
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
                var snapshot = await ReadSnapshotAsync(cancellationToken);
                var removed = snapshot.CheckIns.RemoveAll(checkIn =>
                    string.Equals(checkIn.Id, id, StringComparison.Ordinal));
                if (removed == 0)
                {
                    return;
                }

                if (await TryCommitAsync(
                        snapshot.Raw,
                        snapshot.CheckIns,
                        cancellationToken))
                {
                    return;
                }
            }

            throw new RepositoryConcurrencyException(
                RepositoryStorageKeys.CheckIns,
                RepositoryJson.MaxMutationAttempts);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= RepositoryJson.MaxMutationAttempts; attempt++)
            {
                var snapshot = await ReadSnapshotAsync(cancellationToken);
                if (await TryCommitAsync(
                        snapshot.Raw,
                        [],
                        cancellationToken))
                {
                    return;
                }
            }

            throw new RepositoryConcurrencyException(
                RepositoryStorageKeys.CheckIns,
                RepositoryJson.MaxMutationAttempts);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    private async Task<(string? Raw, List<CheckIn> CheckIns)> ReadSnapshotAsync(
        CancellationToken cancellationToken)
    {
        var snapshot = await RepositoryJson.ReadListSnapshotAsync<CheckInStorageModel>(
            _storage,
            RepositoryStorageKeys.CheckIns,
            cancellationToken);
        var checkIns = snapshot.Value
            .Select(checkIn => checkIn is null
                ? throw new InvalidDataException(
                    "Stored check-ins contain a null record.")
                : checkIn.ToDomain())
            .ToList();
        EnsureUniqueIds(checkIns);
        return (snapshot.Raw, checkIns);
    }

    private async Task<List<CheckIn>> ReadAsync(
        CancellationToken cancellationToken)
    {
        var snapshot = await ReadSnapshotAsync(cancellationToken);
        return snapshot.CheckIns;
    }

    private ValueTask<bool> TryCommitAsync(
        string? expectedRaw,
        IReadOnlyCollection<CheckIn> checkIns,
        CancellationToken cancellationToken)
    {
        var values = Order(checkIns)
            .Select(CheckInStorageModel.FromDomain)
            .ToArray();
        return RepositoryJson.TryWriteAsync(
            _storage,
            RepositoryStorageKeys.CheckIns,
            expectedRaw,
            values,
            cancellationToken);
    }

    private static void ThrowIfDateIsOccupied(
        IEnumerable<CheckIn> checkIns,
        CheckIn candidate,
        string? currentId = null)
    {
        var existing = checkIns.FirstOrDefault(checkIn =>
            checkIn.EntryDate == candidate.EntryDate
            && !string.Equals(checkIn.Id, currentId, StringComparison.Ordinal));

        if (existing is not null)
        {
            throw new DuplicateEntryDateException(
                candidate.EntryDate,
                existing.Id);
        }
    }

    private static void EnsureUniqueIds(IReadOnlyList<CheckIn> checkIns)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var entryDates = new HashSet<DateOnly>();
        foreach (var checkIn in checkIns)
        {
            if (!ids.Add(checkIn.Id))
            {
                throw new InvalidDataException(
                    $"Stored check-ins contain duplicate id '{checkIn.Id}'.");
            }

            if (!entryDates.Add(checkIn.EntryDate))
            {
                throw new InvalidDataException(
                    $"Stored check-ins contain duplicate entry date '{checkIn.EntryDate:yyyy-MM-dd}'.");
            }
        }
    }

    private static IReadOnlyList<CheckIn> Order(IEnumerable<CheckIn> checkIns)
    {
        return checkIns
            .OrderByDescending(checkIn => checkIn.EntryDate)
            .ThenBy(checkIn => checkIn.Id, StringComparer.Ordinal)
            .ToArray();
    }
}
