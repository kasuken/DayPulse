using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure.Storage;

namespace DayPulse.Infrastructure.Repositories;

/// <summary>
/// Persists application settings in browser local storage.
/// </summary>
public sealed class BrowserSettingsRepository : ISettingsRepository
{
    private readonly IAtomicClientStorage _storage;
    private static readonly SemaphoreSlim StorageGate = new(1, 1);

    /// <summary>
    /// Creates a browser-backed settings repository.
    /// </summary>
    /// <param name="storage">The browser storage implementation.</param>
    public BrowserSettingsRepository(IAtomicClientStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    /// <inheritdoc />
    public async Task<AppSettings> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            var stored = await RepositoryJson.ReadAsync<AppSettingsStorageModel>(
                _storage,
                RepositoryStorageKeys.Settings,
                cancellationToken);
            return stored?.ToDomain() ?? new AppSettings();
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(
        AppSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= RepositoryJson.MaxMutationAttempts; attempt++)
            {
                var currentValue = await _storage.GetAsync(
                    RepositoryStorageKeys.Settings,
                    cancellationToken);

                if (await RepositoryJson.TryWriteAsync(
                        _storage,
                        RepositoryStorageKeys.Settings,
                        currentValue,
                        AppSettingsStorageModel.FromDomain(settings),
                        cancellationToken))
                {
                    return;
                }
            }

            throw new RepositoryConcurrencyException(
                RepositoryStorageKeys.Settings,
                RepositoryJson.MaxMutationAttempts);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<AppSettings> UpdateAsync(
        Func<AppSettings, AppSettings> update,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);

        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= RepositoryJson.MaxMutationAttempts; attempt++)
            {
                var currentRaw = await _storage.GetAsync(
                    RepositoryStorageKeys.Settings,
                    cancellationToken);
                var currentSettings = currentRaw is null
                    ? new AppSettings()
                    : RepositoryJson.Deserialize<AppSettingsStorageModel>(
                        currentRaw,
                        RepositoryStorageKeys.Settings).ToDomain();
                var updatedSettings = update(currentSettings)
                    ?? throw new InvalidOperationException(
                        "The settings update function returned null.");

                if (await RepositoryJson.TryWriteAsync(
                        _storage,
                        RepositoryStorageKeys.Settings,
                        currentRaw,
                        AppSettingsStorageModel.FromDomain(updatedSettings),
                        cancellationToken))
                {
                    return updatedSettings;
                }
            }

            throw new RepositoryConcurrencyException(
                RepositoryStorageKeys.Settings,
                RepositoryJson.MaxMutationAttempts);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await StorageGate.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= RepositoryJson.MaxMutationAttempts; attempt++)
            {
                var currentValue = await _storage.GetAsync(
                    RepositoryStorageKeys.Settings,
                    cancellationToken);

                if (currentValue is null)
                {
                    return;
                }

                if (await _storage.TryRemoveAsync(
                        RepositoryStorageKeys.Settings,
                        currentValue,
                        cancellationToken))
                {
                    return;
                }
            }

            throw new RepositoryConcurrencyException(
                RepositoryStorageKeys.Settings,
                RepositoryJson.MaxMutationAttempts);
        }
        finally
        {
            StorageGate.Release();
        }
    }
}
