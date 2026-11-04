using DayPulse.Core;

namespace DayPulse.Tests;

public sealed class DailyReminderTests
{
    [Theory]
    [InlineData(false, 20, 1, false, false)]
    [InlineData(true, 19, 59, false, false)]
    [InlineData(true, 20, 0, false, true)]
    [InlineData(true, 23, 59, false, true)]
    [InlineData(true, 20, 1, true, false)]
    public void Reminders_require_enabled_preferences_time_and_no_entry(
        bool enabled, int hour, int minute, bool hasEntry, bool expected)
    {
        var now = new DateTime(2026, 11, 4, hour, minute, 0);
        Assert.Equal(expected, DailyReminderSchedule.IsDue(
            enabled, new TimeOnly(20, 0), now, hasEntry, null));
    }

    [Fact]
    public void A_reminder_is_not_repeated_for_the_same_local_date()
    {
        var now = new DateTime(2026, 11, 4, 20, 1, 0);
        Assert.False(DailyReminderSchedule.IsDue(
            true, new TimeOnly(20, 0), now, false, new DateOnly(2026, 11, 4)));
        Assert.True(DailyReminderSchedule.IsDue(
            true, new TimeOnly(20, 0), now, false, new DateOnly(2026, 11, 3)));
    }

    [Fact]
    public void Midnight_schedule_uses_the_new_calendar_day()
    {
        Assert.True(DailyReminderSchedule.IsDue(
            true, TimeOnly.MinValue, new DateTime(2027, 1, 1),
            false, new DateOnly(2026, 12, 31)));
    }
}
