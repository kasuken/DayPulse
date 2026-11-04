using DayPulse.Core.Domain;

namespace DayPulse.Tests.Domain;

public sealed class InsightsStatisticsServiceTests
{
    private static readonly DateOnly Today = new(2026, 11, 4);

    [Fact]
    public void Calculate_UsesInclusiveCalendarWindowsAndExcludesOlderCheckIns()
    {
        var statistics = InsightsStatisticsService.Calculate(
        [
            CreateCheckIn(Today, mood: 5),
            CreateCheckIn(Today.AddDays(-6), mood: 3),
            CreateCheckIn(Today.AddDays(-7), mood: -5),
            CreateCheckIn(Today.AddDays(-29), mood: -2),
            CreateCheckIn(Today.AddDays(-30), mood: -4),
            CreateCheckIn(Today.AddDays(-89), mood: 1),
            CreateCheckIn(Today.AddDays(-90), mood: -1)
        ],
        Today);

        Assert.Equal(2, statistics.ForPeriod(InsightsPeriod.SevenDays).CheckInCount);
        Assert.Equal(4, statistics.ForPeriod(InsightsPeriod.ThirtyDays).CheckInCount);
        Assert.Equal(6, statistics.ForPeriod(InsightsPeriod.NinetyDays).CheckInCount);
        Assert.Equal(Today.AddDays(-6), statistics.ForPeriod(InsightsPeriod.SevenDays).StartDate);
        Assert.Equal(Today, statistics.ForPeriod(InsightsPeriod.NinetyDays).EndDate);
    }

    [Fact]
    public void Calculate_AveragesOptionalMetricsOnlyAcrossRecordedValuesAndReturnsTotals()
    {
        var statistics = InsightsStatisticsService.Calculate(
        [
            CreateCheckIn(Today.AddDays(-1), mood: -1, energy: 2, anxiety: null, focus: 6, sleepHours: 7.5, activityIds: ["walk", "read"]),
            CreateCheckIn(Today, mood: 2, energy: null, anxiety: 4, focus: 8, sleepHours: null, activityIds: ["read"])
        ],
        Today).ForPeriod(InsightsPeriod.SevenDays);

        Assert.Equal(0.5m, statistics.AverageMood);
        Assert.Equal(2m, statistics.AverageEnergy);
        Assert.Equal(4m, statistics.AverageAnxiety);
        Assert.Equal(7m, statistics.AverageFocus);
        Assert.Equal(7.5m, statistics.AverageSleepHours);
        Assert.Equal(7.5m, statistics.TotalSleepHours);
        Assert.Equal(3, statistics.ActivitySelections);
    }

    [Fact]
    public void Calculate_EmptyCheckIns_ReturnsNoAveragesAndNoExtrema()
    {
        var summary = InsightsStatisticsService.Calculate([], Today)
            .ForPeriod(InsightsPeriod.SevenDays);

        Assert.Equal(0, summary.CheckInCount);
        Assert.Null(summary.AverageMood);
        Assert.Null(summary.AverageEnergy);
        Assert.Null(summary.AverageAnxiety);
        Assert.Null(summary.AverageFocus);
        Assert.Null(summary.AverageSleepHours);
        Assert.Null(summary.TotalSleepHours);
        Assert.Null(summary.BestMood);
        Assert.Null(summary.LowestMood);
        Assert.Empty(summary.MoodTrend);
    }

    [Fact]
    public void Calculate_ResolvesTiedMoodExtremaToTheEarliestDate()
    {
        var summary = InsightsStatisticsService.Calculate(
        [
            CreateCheckIn(Today, mood: 4),
            CreateCheckIn(Today.AddDays(-2), mood: 4),
            CreateCheckIn(Today.AddDays(-1), mood: -3),
            CreateCheckIn(Today.AddDays(-3), mood: -3)
        ],
        Today).ForPeriod(InsightsPeriod.SevenDays);

        Assert.Equal(new MoodExtremum(Today.AddDays(-2), 4), summary.BestMood);
        Assert.Equal(new MoodExtremum(Today.AddDays(-3), -3), summary.LowestMood);
        Assert.Equal(
            [Today.AddDays(-3), Today.AddDays(-2), Today.AddDays(-1), Today],
            summary.MoodTrend.Select(point => point.Date));
    }

    [Fact]
    public void Calculate_CurrentStreakCanEndYesterdayWhenTodayHasNoCheckIn()
    {
        var statistics = InsightsStatisticsService.Calculate(
        [
            CreateCheckIn(Today.AddDays(-2)),
            CreateCheckIn(Today.AddDays(-1))
        ],
        Today);

        Assert.Equal(2, statistics.CurrentStreak);
    }

    [Fact]
    public void Calculate_CurrentStreakRestartsAtTodayAfterAMissedYesterday()
    {
        var statistics = InsightsStatisticsService.Calculate(
        [
            CreateCheckIn(Today.AddDays(-2)),
            CreateCheckIn(Today)
        ],
        Today);

        Assert.Equal(1, statistics.CurrentStreak);
    }

    [Fact]
    public void Calculate_LongestStreakStopsAtAMissingCalendarDate()
    {
        var statistics = InsightsStatisticsService.Calculate(
        [
            CreateCheckIn(Today.AddDays(-4)),
            CreateCheckIn(Today.AddDays(-3)),
            CreateCheckIn(Today.AddDays(-1))
        ],
        Today);

        Assert.Equal(1, statistics.CurrentStreak);
        Assert.Equal(2, statistics.LongestStreak);
    }

    [Fact]
    public void Calculate_StreaksBreakAtMissingDatesAndHandleLeapDayAndYearBoundaries()
    {
        var today = new DateOnly(2024, 3, 1);
        var statistics = InsightsStatisticsService.Calculate(
        [
            CreateCheckIn(new DateOnly(2024, 2, 27)),
            CreateCheckIn(new DateOnly(2024, 2, 28)),
            CreateCheckIn(new DateOnly(2024, 2, 29)),
            CreateCheckIn(new DateOnly(2024, 3, 1)),
            CreateCheckIn(new DateOnly(2023, 12, 31)),
            CreateCheckIn(new DateOnly(2024, 1, 1)),
            CreateCheckIn(new DateOnly(2024, 1, 3))
        ],
        today);

        Assert.Equal(4, statistics.CurrentStreak);
        Assert.Equal(4, statistics.LongestStreak);
    }

    [Fact]
    public void Calculate_ConsecutiveDatesAcrossYearBoundaryRemainOneStreak()
    {
        var newYear = new DateOnly(2025, 1, 1);
        var statistics = InsightsStatisticsService.Calculate(
        [
            CreateCheckIn(new DateOnly(2024, 12, 31)),
            CreateCheckIn(newYear)
        ],
        newYear);

        Assert.Equal(2, statistics.CurrentStreak);
        Assert.Equal(2, statistics.LongestStreak);
    }

    [Fact]
    public void Calculate_IgnoresFutureCheckInsForStatisticsAndStreaks()
    {
        var statistics = InsightsStatisticsService.Calculate(
        [
            CreateCheckIn(Today, mood: 1),
            CreateCheckIn(Today.AddDays(1), mood: 5)
        ],
        Today);

        Assert.Equal(1, statistics.CurrentStreak);
        Assert.Equal(1, statistics.LongestStreak);
        Assert.Equal(1, statistics.ForPeriod(InsightsPeriod.SevenDays).CheckInCount);
    }

    [Fact]
    public void Calculate_SaturatesWindowAtDateOnlyMinimum()
    {
        var today = DateOnly.MinValue.AddDays(1);
        var summary = InsightsStatisticsService.Calculate(
        [
            CreateCheckIn(DateOnly.MinValue),
            CreateCheckIn(today)
        ],
        today).ForPeriod(InsightsPeriod.SevenDays);

        Assert.Equal(DateOnly.MinValue, summary.StartDate);
        Assert.Equal(2, summary.CheckInCount);
    }

    private static CheckIn CreateCheckIn(
        DateOnly date,
        int mood = 0,
        int? energy = null,
        int? anxiety = null,
        int? focus = null,
        double? sleepHours = null,
        IEnumerable<string>? activityIds = null)
    {
        return new CheckIn(
            Guid.NewGuid().ToString(),
            date,
            mood,
            energy,
            anxiety,
            focus,
            sleepHours,
            activityIds,
            createdAt: DateTimeOffset.UnixEpoch);
    }
}
