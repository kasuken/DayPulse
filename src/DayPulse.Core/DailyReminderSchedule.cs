namespace DayPulse.Core;

/// <summary>Evaluates a best-effort reminder while the application is open.</summary>
public static class DailyReminderSchedule
{
    /// <summary>Returns whether a local-time reminder is due for a day with no check-in.</summary>
    public static bool IsDue(
        bool enabled, TimeOnly reminderTime, DateTime localNow,
        bool hasCheckIn, DateOnly? lastShownDate)
        => enabled
            && !hasCheckIn
            && lastShownDate != DateOnly.FromDateTime(localNow)
            && TimeOnly.FromDateTime(localNow) >= reminderTime;
}
