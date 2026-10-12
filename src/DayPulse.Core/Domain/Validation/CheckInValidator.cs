namespace DayPulse.Core.Domain;

/// <summary>
/// Validates check-in values without depending on UI or persistence concerns.
/// </summary>
public static class CheckInValidator
{
    /// <summary>
    /// The minimum accepted mood score.
    /// </summary>
    public const int MinimumMood = -5;

    /// <summary>
    /// The maximum accepted mood score.
    /// </summary>
    public const int MaximumMood = 5;

    /// <summary>
    /// The minimum accepted optional metric score.
    /// </summary>
    public const int MinimumMetric = 0;

    /// <summary>
    /// The maximum accepted optional metric score.
    /// </summary>
    public const int MaximumMetric = 10;

    /// <summary>
    /// The minimum accepted sleep duration in hours.
    /// </summary>
    public const double MinimumSleepHours = 0;

    /// <summary>
    /// The maximum accepted sleep duration in hours.
    /// </summary>
    public const double MaximumSleepHours = 15;

    /// <summary>
    /// The maximum number of characters allowed in notes.
    /// </summary>
    public const int MaximumNotesLength = 500;

    /// <summary>
    /// Validates a check-in against the current local calendar date.
    /// </summary>
    /// <param name="checkIn">The check-in to validate.</param>
    /// <param name="existingCheckIns">Optional existing check-ins used for duplicate-date validation.</param>
    public static CheckInValidationResult Validate(
        CheckIn checkIn,
        IEnumerable<CheckIn>? existingCheckIns = null)
    {
        return Validate(checkIn, DateOnly.FromDateTime(DateTime.Now), existingCheckIns);
    }

    /// <summary>
    /// Validates a check-in against a supplied local calendar date.
    /// </summary>
    /// <param name="checkIn">The check-in to validate.</param>
    /// <param name="today">The date that future entries are compared against.</param>
    /// <param name="existingCheckIns">Optional existing check-ins used for duplicate-date validation.</param>
    public static CheckInValidationResult Validate(
        CheckIn checkIn,
        DateOnly today,
        IEnumerable<CheckIn>? existingCheckIns = null)
    {
        ArgumentNullException.ThrowIfNull(checkIn);

        var errors = new List<ValidationError>();

        ValidateMood(checkIn.Mood, errors);
        ValidateMetric(checkIn.Energy, CheckInValidationErrorCode.EnergyOutOfRange, nameof(CheckIn.Energy), errors);
        ValidateMetric(checkIn.Anxiety, CheckInValidationErrorCode.AnxietyOutOfRange, nameof(CheckIn.Anxiety), errors);
        ValidateMetric(checkIn.Focus, CheckInValidationErrorCode.FocusOutOfRange, nameof(CheckIn.Focus), errors);
        ValidateSleepHours(checkIn.SleepHours, errors);
        ValidateNotes(checkIn.Notes, errors);
        ValidateEntryDate(checkIn.EntryDate, today, errors);

        if (existingCheckIns is not null && DuplicateEntryDateValidator.IsDuplicate(checkIn, existingCheckIns))
        {
            errors.Add(new ValidationError(
                CheckInValidationErrorCode.DuplicateEntryDate,
                "Only one check-in is allowed per calendar date.",
                nameof(CheckIn.EntryDate)));
        }

        return new CheckInValidationResult(errors);
    }

    /// <summary>
    /// Validates a check-in using the date represented by a timestamp.
    /// </summary>
    /// <param name="checkIn">The check-in to validate.</param>
    /// <param name="now">The timestamp whose calendar date is used as today.</param>
    /// <param name="existingCheckIns">Optional existing check-ins used for duplicate-date validation.</param>
    public static CheckInValidationResult Validate(
        CheckIn checkIn,
        DateTimeOffset now,
        IEnumerable<CheckIn>? existingCheckIns = null)
    {
        return Validate(checkIn, DateOnly.FromDateTime(now.Date), existingCheckIns);
    }

    /// <summary>
    /// Validates a check-in using the date represented by a timestamp.
    /// </summary>
    /// <param name="checkIn">The check-in to validate.</param>
    /// <param name="now">The timestamp whose local calendar date is used as today.</param>
    /// <param name="existingCheckIns">Optional existing check-ins used for duplicate-date validation.</param>
    public static CheckInValidationResult Validate(
        CheckIn checkIn,
        DateTime now,
        IEnumerable<CheckIn>? existingCheckIns = null)
    {
        return Validate(checkIn, DateOnly.FromDateTime(now), existingCheckIns);
    }

    /// <summary>
    /// Validates a check-in and throws when any rule fails.
    /// </summary>
    /// <param name="checkIn">The check-in to validate.</param>
    /// <param name="today">The date that future entries are compared against.</param>
    /// <param name="existingCheckIns">Optional existing check-ins used for duplicate-date validation.</param>
    public static void ValidateOrThrow(
        CheckIn checkIn,
        DateOnly? today = null,
        IEnumerable<CheckIn>? existingCheckIns = null)
    {
        var result = today is null
            ? Validate(checkIn, existingCheckIns)
            : Validate(checkIn, today.Value, existingCheckIns);

        result.ThrowIfInvalid();
    }

    private static void ValidateMood(int mood, ICollection<ValidationError> errors)
    {
        if (mood is < MinimumMood or > MaximumMood)
        {
            errors.Add(new ValidationError(
                CheckInValidationErrorCode.MoodOutOfRange,
                "Mood must be between -5 and 5.",
                nameof(CheckIn.Mood)));
        }
    }

    private static void ValidateMetric(
        int? value,
        CheckInValidationErrorCode code,
        string propertyName,
        ICollection<ValidationError> errors)
    {
        if (value is < MinimumMetric or > MaximumMetric)
        {
            errors.Add(new ValidationError(
                code,
                $"{propertyName} must be between 0 and 10 when provided.",
                propertyName));
        }
    }

    private static void ValidateSleepHours(double? sleepHours, ICollection<ValidationError> errors)
    {
        if (sleepHours is null)
        {
            return;
        }

        var value = sleepHours.Value;
        if (!double.IsFinite(value) || value is < MinimumSleepHours or > MaximumSleepHours)
        {
            errors.Add(new ValidationError(
                CheckInValidationErrorCode.SleepHoursOutOfRange,
                "Sleep hours must be between 0 and 15.",
                nameof(CheckIn.SleepHours)));
            return;
        }

        var doubled = (decimal)value * 2m;
        if (decimal.Truncate(doubled) != doubled)
        {
            errors.Add(new ValidationError(
                CheckInValidationErrorCode.SleepHoursNotInHalfHourIncrements,
                "Sleep hours must be in 0.5-hour increments.",
                nameof(CheckIn.SleepHours)));
        }
    }

    private static void ValidateNotes(string? notes, ICollection<ValidationError> errors)
    {
        if (notes is not null && notes.Length > MaximumNotesLength)
        {
            errors.Add(new ValidationError(
                CheckInValidationErrorCode.NotesTooLong,
                "Notes must be 500 characters or fewer.",
                nameof(CheckIn.Notes)));
        }
    }

    private static void ValidateEntryDate(
        DateOnly entryDate,
        DateOnly today,
        ICollection<ValidationError> errors)
    {
        if (entryDate == DateOnly.MinValue)
        {
            errors.Add(new ValidationError(
                CheckInValidationErrorCode.EntryDateInvalid,
                "Entry date must be a valid calendar date.",
                nameof(CheckIn.EntryDate)));
        }
        else if (entryDate > today)
        {
            errors.Add(new ValidationError(
                CheckInValidationErrorCode.EntryDateInFuture,
                "Entry date cannot be in the future.",
                nameof(CheckIn.EntryDate)));
        }
    }
}
