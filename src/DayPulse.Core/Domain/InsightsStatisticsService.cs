namespace DayPulse.Core.Domain;

/// <summary>
/// Calculates deterministic, non-clinical summaries from locally stored check-ins.
/// </summary>
public static class InsightsStatisticsService
{
    private static readonly InsightsPeriod[] SupportedPeriods =
    [
        InsightsPeriod.SevenDays,
        InsightsPeriod.ThirtyDays,
        InsightsPeriod.NinetyDays
    ];

    /// <summary>
    /// Calculates period summaries and streaks using local calendar dates.
    /// </summary>
    /// <param name="checkIns">The check-ins to summarize.</param>
    /// <param name="today">The current local calendar date.</param>
    /// <returns>Statistics for the supplied check-ins and date.</returns>
    public static InsightsStatistics Calculate(
        IReadOnlyCollection<CheckIn> checkIns,
        DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(checkIns);

        var pastCheckIns = checkIns
            .Where(checkIn => checkIn.EntryDate <= today)
            .ToArray();
        var dates = pastCheckIns
            .Select(checkIn => checkIn.EntryDate)
            .Distinct()
            .Order()
            .ToArray();

        return new InsightsStatistics(
            CalculateCurrentStreak(dates, today),
            CalculateLongestStreak(dates),
            SupportedPeriods
                .Select(period => CalculatePeriod(pastCheckIns, today, period))
                .ToArray());
    }

    private static InsightsPeriodStatistics CalculatePeriod(
        IReadOnlyCollection<CheckIn> checkIns,
        DateOnly today,
        InsightsPeriod period)
    {
        var dayCount = (int)period;
        var firstDayNumber = Math.Max(DateOnly.MinValue.DayNumber, today.DayNumber - dayCount + 1);
        var startDate = DateOnly.FromDayNumber(firstDayNumber);
        var entries = checkIns
            .Where(checkIn => checkIn.EntryDate >= startDate && checkIn.EntryDate <= today)
            .OrderBy(checkIn => checkIn.EntryDate)
            .ToArray();
        var sleepValues = entries
            .Select(checkIn => checkIn.SleepHours)
            .Where(value => value.HasValue)
            .Select(value => (decimal)value.GetValueOrDefault())
            .ToArray();

        return new InsightsPeriodStatistics(
            period,
            startDate,
            today,
            entries.Length,
            Average(entries, checkIn => checkIn.Mood),
            Average(entries, checkIn => checkIn.Energy),
            Average(entries, checkIn => checkIn.Anxiety),
            Average(entries, checkIn => checkIn.Focus),
            Average(sleepValues),
            sleepValues.Length == 0 ? null : sleepValues.Sum(),
            entries.Sum(checkIn => checkIn.ActivityIds.Count),
            entries
                .Select(checkIn => new MoodDataPoint(checkIn.EntryDate, checkIn.Mood))
                .ToArray(),
            entries.Length == 0
                ? null
                : new MoodExtremum(
                    entries.OrderByDescending(checkIn => checkIn.Mood)
                        .ThenBy(checkIn => checkIn.EntryDate)
                        .First().EntryDate,
                    entries.Max(checkIn => checkIn.Mood)),
            entries.Length == 0
                ? null
                : new MoodExtremum(
                    entries.OrderBy(checkIn => checkIn.Mood)
                        .ThenBy(checkIn => checkIn.EntryDate)
                        .First().EntryDate,
                    entries.Min(checkIn => checkIn.Mood)));
    }

    private static decimal? Average(
        IReadOnlyCollection<CheckIn> checkIns,
        Func<CheckIn, int?> selector)
    {
        var values = checkIns
            .Select(selector)
            .Where(value => value.HasValue)
            .Select(value => (decimal)value.GetValueOrDefault())
            .ToArray();

        return Average(values);
    }

    private static decimal? Average(IReadOnlyCollection<decimal> values)
    {
        return values.Count == 0 ? null : values.Average();
    }

    private static int CalculateCurrentStreak(IReadOnlyList<DateOnly> dates, DateOnly today)
    {
        var streakEnd = dates.Contains(today)
            ? today
            : today == DateOnly.MinValue
                ? (DateOnly?)null
                : today.AddDays(-1);
        if (streakEnd is null || !dates.Contains(streakEnd.Value))
        {
            return 0;
        }

        var count = 0;
        var expectedDate = streakEnd.Value;
        for (var index = dates.Count - 1; index >= 0; index--)
        {
            if (dates[index] != expectedDate)
            {
                if (dates[index] < expectedDate)
                {
                    break;
                }

                continue;
            }

            count++;
            if (expectedDate == DateOnly.MinValue)
            {
                break;
            }

            expectedDate = expectedDate.AddDays(-1);
        }

        return count;
    }

    private static int CalculateLongestStreak(IReadOnlyList<DateOnly> dates)
    {
        var longest = 0;
        var current = 0;
        DateOnly? previousDate = null;

        foreach (var date in dates)
        {
            current = previousDate is not null
                && previousDate.Value.DayNumber + 1 == date.DayNumber
                    ? current + 1
                    : 1;
            longest = Math.Max(longest, current);
            previousDate = date;
        }

        return longest;
    }
}
