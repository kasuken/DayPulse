namespace DayPulse.Core.Domain;

/// <summary>
/// The inclusive calendar window used to summarize check-ins.
/// </summary>
public enum InsightsPeriod
{
    /// <summary>Today and the six preceding calendar days.</summary>
    SevenDays = 7,

    /// <summary>Today and the 29 preceding calendar days.</summary>
    ThirtyDays = 30,

    /// <summary>Today and the 89 preceding calendar days.</summary>
    NinetyDays = 90
}

/// <summary>
/// A mood value recorded on one local calendar date.
/// </summary>
/// <param name="Date">The check-in date.</param>
/// <param name="Mood">The mood score recorded for that date.</param>
public sealed record MoodDataPoint(DateOnly Date, int Mood);

/// <summary>
/// A best or lowest mood observation for a selected period.
/// </summary>
/// <param name="Date">The date of the observation.</param>
/// <param name="Mood">The observed mood score.</param>
public sealed record MoodExtremum(DateOnly Date, int Mood);

/// <summary>
/// Summary statistics for one inclusive calendar period.
/// </summary>
/// <param name="Period">The period represented by the summary.</param>
/// <param name="StartDate">The first included calendar date.</param>
/// <param name="EndDate">The last included calendar date.</param>
/// <param name="CheckInCount">The number of check-ins in the period.</param>
/// <param name="AverageMood">The average mood, or <see langword="null"/> when there are no check-ins.</param>
/// <param name="AverageEnergy">The average of recorded energy values.</param>
/// <param name="AverageAnxiety">The average of recorded anxiety values.</param>
/// <param name="AverageFocus">The average of recorded focus values.</param>
/// <param name="AverageSleepHours">The average of recorded sleep durations in hours.</param>
/// <param name="TotalSleepHours">The sum of recorded sleep durations in hours.</param>
/// <param name="ActivitySelections">The total number of activity selections in the period.</param>
/// <param name="MoodTrend">Recorded mood observations ordered by date.</param>
/// <param name="BestMood">The highest mood observation, with ties resolved to the earliest date.</param>
/// <param name="LowestMood">The lowest mood observation, with ties resolved to the earliest date.</param>
public sealed record InsightsPeriodStatistics(
    InsightsPeriod Period,
    DateOnly StartDate,
    DateOnly EndDate,
    int CheckInCount,
    decimal? AverageMood,
    decimal? AverageEnergy,
    decimal? AverageAnxiety,
    decimal? AverageFocus,
    decimal? AverageSleepHours,
    decimal? TotalSleepHours,
    int ActivitySelections,
    IReadOnlyList<MoodDataPoint> MoodTrend,
    MoodExtremum? BestMood,
    MoodExtremum? LowestMood);

/// <summary>
/// All-time streaks and period summaries calculated from local-calendar check-ins.
/// </summary>
/// <param name="CurrentStreak">The consecutive streak ending today or, if absent, yesterday.</param>
/// <param name="LongestStreak">The longest sequence of consecutive calendar dates with check-ins up to today.</param>
/// <param name="Periods">Summaries for the supported period lengths.</param>
public sealed record InsightsStatistics(
    int CurrentStreak,
    int LongestStreak,
    IReadOnlyList<InsightsPeriodStatistics> Periods)
{
    /// <summary>
    /// Gets the summary for the requested period.
    /// </summary>
    /// <param name="period">The requested inclusive calendar period.</param>
    /// <returns>The matching period summary.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The period is not supported.</exception>
    public InsightsPeriodStatistics ForPeriod(InsightsPeriod period)
    {
        return Periods.FirstOrDefault(summary => summary.Period == period)
            ?? throw new ArgumentOutOfRangeException(nameof(period), period, "Unsupported insights period.");
    }
}
