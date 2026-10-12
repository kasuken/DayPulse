using DayPulse.Core.Domain;

namespace DayPulse.Core.Repositories;

/// <summary>
/// Provides persistence operations for non-sensitive application settings.
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Loads the stored settings, or the domain defaults when no settings exist.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the supplied settings.
    /// </summary>
    /// <param name="settings">The settings to store.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task SaveAsync(
        AppSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the latest stored settings and persists the result.
    /// </summary>
    /// <param name="update">The function that produces updated settings from the latest value.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The settings produced by <paramref name="update"/>.</returns>
    /// <remarks>
    /// Implementations should apply the update to the value read immediately before each
    /// conditional write so unrelated changes from another browser context are preserved.
    /// </remarks>
    async Task<AppSettings> UpdateAsync(
        Func<AppSettings, AppSettings> update,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);

        var currentSettings = await LoadAsync(cancellationToken);
        var updatedSettings = update(currentSettings)
            ?? throw new InvalidOperationException(
                "The settings update function returned null.");
        await SaveAsync(updatedSettings, cancellationToken);
        return updatedSettings;
    }

    /// <summary>
    /// Removes stored settings so the next load returns domain defaults.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task ResetAsync(CancellationToken cancellationToken = default);
}
