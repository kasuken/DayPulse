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
    /// Removes stored settings so the next load returns domain defaults.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task ResetAsync(CancellationToken cancellationToken = default);
}
