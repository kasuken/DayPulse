using DayPulse.Core.Domain;

namespace DayPulse.Tests.Domain;

public sealed class DomainEntitiesTests
{
    [Fact]
    public void CheckIn_ConstructorCopiesActivitiesAndStoresTimestampsAsUtc()
    {
        var activities = new List<string> { "walk" };
        var createdAt = new DateTimeOffset(2026, 10, 12, 12, 0, 0, TimeSpan.FromHours(2));
        var updatedAt = new DateTimeOffset(2026, 10, 12, 13, 0, 0, TimeSpan.FromHours(2));

        var checkIn = new CheckIn(
            "check-in",
            new DateOnly(2026, 10, 12),
            mood: 2,
            activityIds: activities,
            createdAt: createdAt,
            updatedAt: updatedAt);

        activities.Add("reading");

        Assert.Single(checkIn.ActivityIds);
        Assert.Equal(createdAt.ToUniversalTime(), checkIn.CreatedAt);
        Assert.Equal(updatedAt.ToUniversalTime(), checkIn.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, checkIn.CreatedAt.Offset);
        Assert.Equal(TimeSpan.Zero, checkIn.UpdatedAt.Offset);
    }

    [Fact]
    public void Activity_StoresAllDomainProperties()
    {
        var createdAt = new DateTimeOffset(2026, 10, 12, 12, 0, 0, TimeSpan.FromHours(1));
        var updatedAt = createdAt.AddHours(1);

        var activity = new Activity(
            "activity",
            "Walking",
            isDefault: true,
            isActive: false,
            sortOrder: 4,
            createdAt: createdAt,
            updatedAt: updatedAt);

        Assert.Equal("activity", activity.Id);
        Assert.Equal("Walking", activity.Name);
        Assert.True(activity.IsDefault);
        Assert.False(activity.IsActive);
        Assert.Equal(4, activity.SortOrder);
        Assert.Equal(createdAt.ToUniversalTime(), activity.CreatedAt);
        Assert.Equal(updatedAt.ToUniversalTime(), activity.UpdatedAt);
    }

    [Fact]
    public void Activity_ConstructorRejectsBlankIdentityAndName()
    {
        Assert.Throws<ArgumentException>(() => new Activity("", "Walking"));
        Assert.Throws<ArgumentException>(() => new Activity("activity", " "));
    }

    [Fact]
    public void AppSettings_DefaultsEnableMetricsAndUseSystemTheme()
    {
        var settings = new AppSettings();

        Assert.False(settings.OnboardingCompleted);
        Assert.Equal(ThemeMode.System, settings.ThemeMode);
        Assert.False(settings.ReminderEnabled);
        Assert.Equal(new TimeOnly(20, 0), settings.ReminderTime);
        Assert.True(settings.EnergyEnabled);
        Assert.True(settings.AnxietyEnabled);
        Assert.True(settings.FocusEnabled);
        Assert.True(settings.SleepEnabled);
    }

    [Fact]
    public void AppSettings_ConstructorStoresReminderAndMetricPreferences()
    {
        var settings = new AppSettings(
            onboardingCompleted: true,
            themeMode: ThemeMode.Dark,
            reminderEnabled: true,
            reminderTime: new TimeOnly(7, 30),
            energyEnabled: false,
            anxietyEnabled: true,
            focusEnabled: false,
            sleepEnabled: true);

        Assert.True(settings.OnboardingCompleted);
        Assert.Equal(ThemeMode.Dark, settings.ThemeMode);
        Assert.True(settings.ReminderEnabled);
        Assert.Equal(new TimeOnly(7, 30), settings.ReminderTime);
        Assert.False(settings.EnergyEnabled);
        Assert.True(settings.AnxietyEnabled);
        Assert.False(settings.FocusEnabled);
        Assert.True(settings.SleepEnabled);
    }

    [Fact]
    public void AppSettings_ConstructorRejectsUnknownThemeMode()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AppSettings(themeMode: (ThemeMode)99));
    }
}
