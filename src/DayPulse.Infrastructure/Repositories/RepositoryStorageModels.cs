using DayPulse.Core.Domain;

namespace DayPulse.Infrastructure.Repositories;

internal sealed record CheckInStorageModel(
    string? Id,
    DateOnly? EntryDate,
    int? Mood,
    int? Energy,
    int? Anxiety,
    int? Focus,
    double? SleepHours,
    IReadOnlyList<string>? ActivityIds,
    string? Notes,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    internal static CheckInStorageModel FromDomain(CheckIn checkIn)
    {
        return new CheckInStorageModel(
            checkIn.Id,
            checkIn.EntryDate,
            checkIn.Mood,
            checkIn.Energy,
            checkIn.Anxiety,
            checkIn.Focus,
            checkIn.SleepHours,
            checkIn.ActivityIds,
            checkIn.Notes,
            checkIn.CreatedAt,
            checkIn.UpdatedAt);
    }

    internal CheckIn ToDomain()
    {
        var id = Id
            ?? throw new InvalidDataException(
                "Stored check-in is missing its identifier.");
        var entryDate = EntryDate
            ?? throw new InvalidDataException(
                "Stored check-in is missing its entry date.");
        var mood = Mood
            ?? throw new InvalidDataException(
                "Stored check-in is missing its mood.");
        var activityIds = ActivityIds
            ?? throw new InvalidDataException(
                "Stored check-in is missing its activity identifiers.");
        var createdAt = CreatedAt
            ?? throw new InvalidDataException(
                "Stored check-in is missing its creation timestamp.");
        var updatedAt = UpdatedAt
            ?? throw new InvalidDataException(
                "Stored check-in is missing its update timestamp.");

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidDataException(
                "Stored check-in has a blank identifier.");
        }

        if (entryDate == DateOnly.MinValue)
        {
            throw new InvalidDataException(
                "Stored check-in has an invalid entry date.");
        }

        if (createdAt == default || updatedAt == default || updatedAt < createdAt)
        {
            throw new InvalidDataException(
                "Stored check-in has invalid timestamp metadata.");
        }

        if (activityIds.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidDataException(
                "Stored check-in contains a blank activity identifier.");
        }

        var checkIn = new CheckIn(
            id,
            entryDate,
            mood,
            Energy,
            Anxiety,
            Focus,
            SleepHours,
            activityIds,
            Notes,
            createdAt,
            updatedAt);
        var validation = CheckInValidator.Validate(checkIn, DateOnly.MaxValue);
        if (validation.IsInvalid)
        {
            throw new InvalidDataException(
                $"Stored check-in '{checkIn.Id}' failed domain validation: "
                + string.Join(" ", validation.Errors.Select(error => error.Message)));
        }

        return checkIn;
    }
}

internal sealed record ActivityStorageModel(
    string? Id,
    string? Name,
    bool? IsDefault,
    bool? IsActive,
    int? SortOrder,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    internal static ActivityStorageModel FromDomain(Activity activity)
    {
        return new ActivityStorageModel(
            activity.Id,
            activity.Name,
            activity.IsDefault,
            activity.IsActive,
            activity.SortOrder,
            activity.CreatedAt,
            activity.UpdatedAt);
    }

    internal Activity ToDomain()
    {
        var id = Id
            ?? throw new InvalidDataException(
                "Stored activity is missing its identifier.");
        var name = Name
            ?? throw new InvalidDataException(
                "Stored activity is missing its name.");
        var isDefault = IsDefault
            ?? throw new InvalidDataException(
                "Stored activity is missing its built-in flag.");
        var isActive = IsActive
            ?? throw new InvalidDataException(
                "Stored activity is missing its active flag.");
        var sortOrder = SortOrder
            ?? throw new InvalidDataException(
                "Stored activity is missing its sort order.");
        var createdAt = CreatedAt
            ?? throw new InvalidDataException(
                "Stored activity is missing its creation timestamp.");
        var updatedAt = UpdatedAt
            ?? throw new InvalidDataException(
                "Stored activity is missing its update timestamp.");

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidDataException(
                "Stored activity has a blank identifier or name.");
        }

        if (createdAt == default || updatedAt == default || updatedAt < createdAt)
        {
            throw new InvalidDataException(
                $"Stored activity '{id}' has invalid timestamp metadata.");
        }

        return new Activity(
            id,
            name,
            isDefault,
            isActive,
            sortOrder,
            createdAt,
            updatedAt);
    }
}

internal sealed record AppSettingsStorageModel(
    bool? OnboardingCompleted,
    ThemeMode? ThemeMode,
    bool? ReminderEnabled,
    TimeOnly? ReminderTime,
    bool? EnergyEnabled,
    bool? AnxietyEnabled,
    bool? FocusEnabled,
    bool? SleepEnabled)
{
    internal static AppSettingsStorageModel FromDomain(AppSettings settings)
    {
        return new AppSettingsStorageModel(
            settings.OnboardingCompleted,
            settings.ThemeMode,
            settings.ReminderEnabled,
            settings.ReminderTime,
            settings.EnergyEnabled,
            settings.AnxietyEnabled,
            settings.FocusEnabled,
            settings.SleepEnabled);
    }

    internal AppSettings ToDomain()
    {
        if (OnboardingCompleted is null
            || ThemeMode is null
            || ReminderEnabled is null
            || ReminderTime is null
            || EnergyEnabled is null
            || AnxietyEnabled is null
            || FocusEnabled is null
            || SleepEnabled is null)
        {
            throw new InvalidDataException(
                "Stored application settings are missing one or more required fields.");
        }

        return new AppSettings(
            OnboardingCompleted.Value,
            ThemeMode.Value,
            ReminderEnabled.Value,
            ReminderTime.Value,
            EnergyEnabled.Value,
            AnxietyEnabled.Value,
            FocusEnabled.Value,
            SleepEnabled.Value);
    }
}
