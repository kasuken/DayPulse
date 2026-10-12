namespace DayPulse.Core.Domain;

/// <summary>
/// Identifies a check-in validation rule that was violated.
/// </summary>
public enum CheckInValidationErrorCode
{
    /// <summary>
    /// The mood is outside the inclusive -5 through 5 range.
    /// </summary>
    MoodOutOfRange,

    /// <summary>
    /// The energy value is outside the inclusive 0 through 10 range.
    /// </summary>
    EnergyOutOfRange,

    /// <summary>
    /// The anxiety value is outside the inclusive 0 through 10 range.
    /// </summary>
    AnxietyOutOfRange,

    /// <summary>
    /// The focus value is outside the inclusive 0 through 10 range.
    /// </summary>
    FocusOutOfRange,

    /// <summary>
    /// Sleep duration is outside the inclusive 0 through 15 hour range.
    /// </summary>
    SleepHoursOutOfRange,

    /// <summary>
    /// Sleep duration is not a half-hour increment.
    /// </summary>
    SleepHoursNotInHalfHourIncrements,

    /// <summary>
    /// Notes exceed the maximum supported length.
    /// </summary>
    NotesTooLong,

    /// <summary>
    /// The entry date is the sentinel minimum date and is not usable.
    /// </summary>
    EntryDateInvalid,

    /// <summary>
    /// The entry date is after the validation reference date.
    /// </summary>
    EntryDateInFuture,

    /// <summary>
    /// Another check-in already uses the entry date.
    /// </summary>
    DuplicateEntryDate
}
