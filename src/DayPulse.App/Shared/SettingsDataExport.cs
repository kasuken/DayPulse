using System.Globalization;
using System.Text;
using System.Text.Json;
using DayPulse.Core.Domain;

namespace DayPulse.App.Shared;

/// <summary>Produces local, versioned exports of the complete stored wellbeing data.</summary>
public static class SettingsDataExport
{
    /// <summary>Exports check-ins, the full activity catalog and settings as JSON.</summary>
    public static string Json(IReadOnlyList<CheckIn> checkIns, IReadOnlyList<Activity> activities, AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(checkIns);
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(settings);
        return JsonSerializer.Serialize(new { SchemaVersion = 1, CheckIns = checkIns, Activities = activities, Settings = settings },
            new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Exports typed rows for check-ins, activities and settings in a single UTF-8 CSV.
    /// Text cells are quoted and spreadsheet formula prefixes are neutralized.
    /// </summary>
    public static string Csv(IReadOnlyList<CheckIn> checkIns, IReadOnlyList<Activity> activities, AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(checkIns);
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(settings);
        var result = new StringBuilder();
        Row(result, "RecordType", "Id", "EntryDate", "Mood", "Energy", "Anxiety", "Focus", "SleepHours",
            "ActivityIds", "Notes", "Name", "IsDefault", "IsActive", "SortOrder", "CreatedAt", "UpdatedAt",
            "OnboardingCompleted", "ThemeMode", "ReminderEnabled", "ReminderTime", "EnergyEnabled", "AnxietyEnabled", "FocusEnabled", "SleepEnabled");
        foreach (var checkIn in checkIns)
        {
            Row(result, "CheckIn", Text(checkIn.Id), checkIn.EntryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Number(checkIn.Mood), Number(checkIn.Energy), Number(checkIn.Anxiety), Number(checkIn.Focus), Number(checkIn.SleepHours),
                JsonSerializer.Serialize(checkIn.ActivityIds), Text(checkIn.Notes), "", "", "", "",
                checkIn.CreatedAt.ToString("O", CultureInfo.InvariantCulture), checkIn.UpdatedAt.ToString("O", CultureInfo.InvariantCulture),
                "", "", "", "", "", "", "", "");
        }

        foreach (var activity in activities)
        {
            Row(result, "Activity", Text(activity.Id), "", "", "", "", "", "", "", "", Text(activity.Name),
                activity.IsDefault.ToString(), activity.IsActive.ToString(), Number(activity.SortOrder),
                activity.CreatedAt.ToString("O", CultureInfo.InvariantCulture), activity.UpdatedAt.ToString("O", CultureInfo.InvariantCulture),
                "", "", "", "", "", "", "", "");
        }

        Row(result, "Settings", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            settings.OnboardingCompleted.ToString(), settings.ThemeMode.ToString(), settings.ReminderEnabled.ToString(),
            settings.ReminderTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture), settings.EnergyEnabled.ToString(),
            settings.AnxietyEnabled.ToString(), settings.FocusEnabled.ToString(), settings.SleepEnabled.ToString());
        return result.ToString();
    }

    private static string Number<T>(T? value) where T : struct, IFormattable =>
        value?.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Text(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        // Quoting alone does not stop spreadsheet applications evaluating formulas.
        var trimmed = value.AsSpan().TrimStart();
        return trimmed.Length > 0 && trimmed[0] is '=' or '+' or '-' or '@'
            || value[0] is '\t' or '\r' or '\n'
            ? "'" + value : value;
    }

    private static void Row(StringBuilder result, params string[] cells) =>
        result.AppendJoin(',', cells.Select(cell => $"\"{cell.Replace("\"", "\"\"", StringComparison.Ordinal)}\"")).Append("\r\n");
}
