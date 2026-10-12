namespace DayPulse.Core.Domain;

/// <summary>
/// Provides the date-only duplicate rule without requiring a persistence implementation.
/// </summary>
public static class DuplicateEntryDateValidator
{
    /// <summary>
    /// Determines whether an entry date already exists in a sequence of dates.
    /// </summary>
    /// <param name="entryDate">The candidate entry date.</param>
    /// <param name="existingEntryDates">Dates already used by stored check-ins.</param>
    public static bool IsDuplicate(
        DateOnly entryDate,
        IEnumerable<DateOnly> existingEntryDates)
    {
        ArgumentNullException.ThrowIfNull(existingEntryDates);
        return existingEntryDates.Contains(entryDate);
    }

    /// <summary>
    /// Determines whether another check-in already uses the candidate's date.
    /// A check-in with the same non-empty identifier is treated as the current record during updates.
    /// </summary>
    /// <param name="candidate">The candidate check-in.</param>
    /// <param name="existingCheckIns">Check-ins already stored.</param>
    public static bool IsDuplicate(
        CheckIn candidate,
        IEnumerable<CheckIn> existingCheckIns)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(existingCheckIns);

        return existingCheckIns.Any(existing =>
            existing.EntryDate == candidate.EntryDate
            && (string.IsNullOrEmpty(candidate.Id)
                || !string.Equals(existing.Id, candidate.Id, StringComparison.Ordinal)));
    }

    /// <summary>
    /// Returns the duplicate-date validation error when the date is already used.
    /// </summary>
    /// <param name="entryDate">The candidate entry date.</param>
    /// <param name="existingEntryDates">Dates already used by stored check-ins.</param>
    public static ValidationError? Validate(
        DateOnly entryDate,
        IEnumerable<DateOnly> existingEntryDates)
    {
        return IsDuplicate(entryDate, existingEntryDates)
            ? new ValidationError(
                CheckInValidationErrorCode.DuplicateEntryDate,
                "Only one check-in is allowed per calendar date.",
                nameof(CheckIn.EntryDate))
            : null;
    }
}
