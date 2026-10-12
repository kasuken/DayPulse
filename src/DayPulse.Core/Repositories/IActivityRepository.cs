using DayPulse.Core.Domain;

namespace DayPulse.Core.Repositories;

/// <summary>
/// Provides persistence operations for selectable activities.
/// </summary>
public interface IActivityRepository
{
    /// <summary>
    /// Gets every activity, including inactive activities, ordered for display.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task<IReadOnlyList<Activity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active activities ordered for display.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task<IReadOnlyList<Activity>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an activity by its stable identifier.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The matching activity, or <see langword="null"/> when it is absent.</returns>
    Task<Activity?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a custom activity.
    /// </summary>
    /// <param name="activity">The activity to add.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task AddAsync(
        Activity activity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing activity while preserving its original creation timestamp.
    /// </summary>
    /// <param name="activity">The updated activity.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task UpdateAsync(
        Activity activity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets whether an activity is active.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    /// <param name="isActive">Whether the activity should be selectable.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task SetActiveAsync(
        string id,
        bool isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates an activity without deleting it.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task DeactivateAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a custom activity when it exists.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <exception cref="BuiltInActivityDeleteException">
    /// Thrown when the activity is one of the built-in activities.
    /// </exception>
    Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds the built-in activity catalog if it has not been seeded yet.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task SeedDefaultsAsync(CancellationToken cancellationToken = default);
}
