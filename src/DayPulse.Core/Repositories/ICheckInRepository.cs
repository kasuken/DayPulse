using DayPulse.Core.Domain;

namespace DayPulse.Core.Repositories;

/// <summary>
/// Provides persistence operations for wellbeing check-ins.
/// </summary>
public interface ICheckInRepository
{
    /// <summary>
    /// Gets every stored check-in ordered by entry date descending.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task<IReadOnlyList<CheckIn>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets check-ins between two dates, inclusive.
    /// </summary>
    /// <param name="startDate">The first date in the range.</param>
    /// <param name="endDate">The last date in the range.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task<IReadOnlyList<CheckIn>> GetBetweenAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a check-in by its stable identifier.
    /// </summary>
    /// <param name="id">The check-in identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The matching check-in, or <see langword="null"/> when it is absent.</returns>
    Task<CheckIn?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the check-in for a local calendar date.
    /// </summary>
    /// <param name="entryDate">The date to look up.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The matching check-in, or <see langword="null"/> when it is absent.</returns>
    Task<CheckIn?> GetByDateAsync(
        DateOnly entryDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a check-in.
    /// </summary>
    /// <param name="checkIn">The check-in to create.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <exception cref="DuplicateEntryDateException">
    /// Thrown when another check-in already uses the same entry date.
    /// </exception>
    Task CreateAsync(
        CheckIn checkIn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing check-in, preserving its original creation timestamp.
    /// </summary>
    /// <param name="checkIn">The updated check-in. Its identifier must already exist.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <exception cref="DuplicateEntryDateException">
    /// Thrown when another check-in already uses the updated entry date.
    /// </exception>
    /// <exception cref="RepositoryEntityNotFoundException">
    /// Thrown when the check-in identifier does not exist.
    /// </exception>
    Task UpdateAsync(
        CheckIn checkIn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a check-in when it exists.
    /// </summary>
    /// <param name="id">The check-in identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all check-ins.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task DeleteAllAsync(CancellationToken cancellationToken = default);
}
