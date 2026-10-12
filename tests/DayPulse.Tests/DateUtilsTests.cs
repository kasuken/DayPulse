using DayPulse.Core;

namespace DayPulse.Tests;

public sealed class DateUtilsTests
{
    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void FromLocalDateTime_DateTime_PreservesCalendarFields(DateTimeKind kind)
    {
        var localDateTime = new DateTime(2026, 10, 12, 0, 15, 0, kind);

        var result = DateUtils.FromLocalDateTime(localDateTime);

        Assert.Equal(new DateOnly(2026, 10, 12), result);
    }

    [Fact]
    public void FromLocalDateTime_UtcDateTimeRequiresExplicitTimeZoneConversion()
    {
        var utcDateTime = new DateTime(2026, 10, 12, 0, 15, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentException>(
            () => DateUtils.FromLocalDateTime(utcDateTime));
    }

    [Fact]
    public void FromLocalDateTime_DateTimeOffset_PreservesOffsetLocalCalendarDate()
    {
        var localDateTime = new DateTimeOffset(
            2026,
            10,
            12,
            0,
            15,
            0,
            TimeSpan.FromHours(14));

        var result = DateUtils.FromLocalDateTime(localDateTime);

        Assert.Equal(new DateOnly(2026, 10, 12), result);
    }

    [Fact]
    public void FromUtcInstant_DateTime_ConvertsAcrossCalendarBoundary()
    {
        var utcInstant = new DateTime(2026, 1, 1, 0, 30, 0, DateTimeKind.Utc);
        var timeZone = TimeZoneInfo.CreateCustomTimeZone(
            "UTC-05",
            TimeSpan.FromHours(-5),
            "UTC-05",
            "UTC-05");

        var result = DateUtils.FromUtcInstant(utcInstant, timeZone);

        Assert.Equal(new DateOnly(2025, 12, 31), result);
    }

    [Fact]
    public void FromUtcInstant_DateTime_RejectsNonUtcKind()
    {
        var localDateTime = new DateTime(2026, 10, 12, 12, 0, 0, DateTimeKind.Local);

        Assert.Throws<ArgumentException>(
            () => DateUtils.FromUtcInstant(localDateTime, TimeZoneInfo.Utc));
    }

    [Fact]
    public void FromUtcInstant_DateTime_RejectsUnspecifiedKind()
    {
        var unspecifiedDateTime = new DateTime(2026, 10, 12, 12, 0, 0, DateTimeKind.Unspecified);

        Assert.Throws<ArgumentException>(
            () => DateUtils.FromUtcInstant(unspecifiedDateTime, TimeZoneInfo.Utc));
    }

    [Fact]
    public void FromUtcInstant_DateTimeOffset_UsesDestinationTimeZoneRules()
    {
        var utcInstant = new DateTimeOffset(
            2026,
            1,
            1,
            0,
            30,
            0,
            TimeSpan.Zero);
        var timeZone = TimeZoneInfo.CreateCustomTimeZone(
            "UTC+14",
            TimeSpan.FromHours(14),
            "UTC+14",
            "UTC+14");

        var result = DateUtils.FromUtcInstant(utcInstant, timeZone);

        Assert.Equal(new DateOnly(2026, 1, 1), result);
    }

    [Fact]
    public void FromUtcInstant_DateTimeOffset_UsesInstantRatherThanStoredOffsetDate()
    {
        var instantWithNonZeroOffset = new DateTimeOffset(
            2026,
            1,
            1,
            0,
            30,
            0,
            TimeSpan.FromHours(14));

        var result = DateUtils.FromUtcInstant(instantWithNonZeroOffset, TimeZoneInfo.Utc);

        Assert.Equal(new DateOnly(2025, 12, 31), result);
    }

    [Fact]
    public void FromUtcInstant_NullTimeZone_ThrowsArgumentNullException()
    {
        var utcInstant = new DateTimeOffset(2026, 10, 12, 12, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentNullException>(
            () => DateUtils.FromUtcInstant(utcInstant, null!));
    }

    [Fact]
    public void ResolveTimeZone_ResolvesIanaAndWindowsIdentifiers()
    {
        var ianaZone = ResolveAvailableTimeZoneOrSkip("America/New_York");
        var windowsZone = ResolveAvailableTimeZoneOrSkip("Eastern Standard Time");
        var winterInstant = new DateTimeOffset(
            2026,
            1,
            1,
            4,
            30,
            0,
            TimeSpan.Zero);
        var summerInstant = new DateTimeOffset(
            2026,
            7,
            1,
            3,
            30,
            0,
            TimeSpan.Zero);

        Assert.Equal(ianaZone.BaseUtcOffset, windowsZone.BaseUtcOffset);
        Assert.Equal(ianaZone.SupportsDaylightSavingTime, windowsZone.SupportsDaylightSavingTime);
        Assert.Equal(
            DateUtils.FromUtcInstant(winterInstant, ianaZone),
            DateUtils.FromUtcInstant(winterInstant, windowsZone));
        Assert.Equal(
            DateUtils.FromUtcInstant(summerInstant, ianaZone),
            DateUtils.FromUtcInstant(summerInstant, windowsZone));
    }

    [Fact]
    public void ResolveTimeZone_InvalidIdentifierThrowsTimeZoneNotFoundException()
    {
        Assert.Throws<TimeZoneNotFoundException>(
            () => DateUtils.ResolveTimeZone("Not/A-Real-Time-Zone"));
    }

    [Fact]
    public void ResolveTimeZone_BlankIdentifierThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => DateUtils.ResolveTimeZone(" "));
    }

    [Fact]
    public void AreConsecutiveDays_HandlesLeapDayAndYearBoundary()
    {
        Assert.True(DateUtils.AreConsecutiveDays(
            new DateOnly(2024, 2, 28),
            new DateOnly(2024, 2, 29)));
        Assert.True(DateUtils.AreConsecutiveDays(
            new DateOnly(2024, 2, 29),
            new DateOnly(2024, 3, 1)));
        Assert.True(DateUtils.AreConsecutiveDays(
            new DateOnly(2024, 12, 31),
            new DateOnly(2025, 1, 1)));
    }

    [Fact]
    public void AreConsecutiveDays_RequiresOrderedDistinctAdjacentDates()
    {
        var date = new DateOnly(2026, 10, 12);

        Assert.False(DateUtils.AreConsecutiveDays(date, date));
        Assert.False(DateUtils.AreConsecutiveDays(date.AddDays(1), date));
        Assert.False(DateUtils.AreConsecutiveDays(date, date.AddDays(2)));
    }

    [Fact]
    public void GetPast7Days_IncludesCurrentDateInAscendingOrder()
    {
        var result = DateUtils.GetPast7Days(new DateOnly(2026, 3, 1));

        Assert.Equal(7, result.Count);
        Assert.Equal(new DateOnly(2026, 2, 23), result[0]);
        Assert.Equal(new DateOnly(2026, 3, 1), result[^1]);
        AssertAscending(result);
    }

    [Fact]
    public void GetPast30Days_UsesCalendarArithmeticAcrossYearBoundary()
    {
        var result = DateUtils.GetPast30Days(new DateOnly(2025, 1, 5));

        Assert.Equal(30, result.Count);
        Assert.Equal(new DateOnly(2024, 12, 7), result[0]);
        Assert.Equal(new DateOnly(2025, 1, 5), result[^1]);
        AssertAscending(result);
    }

    [Fact]
    public void GetPast90Days_IncludesLeapDayAndIsInclusive()
    {
        var result = DateUtils.GetPast90Days(new DateOnly(2024, 3, 1));

        Assert.Equal(90, result.Count);
        Assert.Equal(new DateOnly(2023, 12, 3), result[0]);
        Assert.Equal(new DateOnly(2024, 2, 29), result[88]);
        Assert.Equal(new DateOnly(2024, 3, 1), result[^1]);
        AssertAscending(result);
    }

    [Fact]
    public void GetPast7Days_BeforeMinimumDateThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => DateUtils.GetPast7Days(DateOnly.MinValue));
    }

    [Fact]
    public void GetCurrentWeek_DefaultsToMondayAndIncludesBothBoundaries()
    {
        var result = DateUtils.GetCurrentWeek(new DateOnly(2026, 10, 14));

        Assert.Equal(7, result.Count);
        Assert.Equal(new DateOnly(2026, 10, 12), result[0]);
        Assert.Equal(new DateOnly(2026, 10, 18), result[^1]);
        AssertAscending(result);
    }

    [Fact]
    public void GetCurrentWeek_AtMaximumDateThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => DateUtils.GetCurrentWeek(DateOnly.MaxValue));
    }

    [Fact]
    public void GetCurrentWeek_CrossesYearBoundaryUsingCalendarDays()
    {
        var result = DateUtils.GetCurrentWeek(new DateOnly(2026, 1, 1));

        Assert.Equal(new DateOnly(2025, 12, 29), result[0]);
        Assert.Equal(new DateOnly(2026, 1, 4), result[^1]);
        AssertAscending(result);
    }

    [Fact]
    public void GetCurrentWeek_UsesRequestedFirstDayOfWeek()
    {
        var result = DateUtils.GetCurrentWeek(
            new DateOnly(2026, 10, 14),
            DayOfWeek.Sunday);

        Assert.Equal(new DateOnly(2026, 10, 11), result[0]);
        Assert.Equal(new DateOnly(2026, 10, 17), result[^1]);
        AssertAscending(result);
    }

    [Fact]
    public void GetCurrentWeek_InvalidFirstDayOfWeekThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => DateUtils.GetCurrentWeek(
                new DateOnly(2026, 10, 14),
                (DayOfWeek)7));
    }

    [Fact]
    public void GetCurrentMonth_HandlesLeapYearFebruary()
    {
        var result = DateUtils.GetCurrentMonth(new DateOnly(2024, 2, 15));

        Assert.Equal(29, result.Count);
        Assert.Equal(new DateOnly(2024, 2, 1), result[0]);
        Assert.Equal(new DateOnly(2024, 2, 29), result[^1]);
        AssertAscending(result);
    }

    [Fact]
    public void GetCurrentMonth_HandlesNonLeapYearFebruary()
    {
        var result = DateUtils.GetCurrentMonth(new DateOnly(2025, 2, 15));

        Assert.Equal(28, result.Count);
        Assert.Equal(new DateOnly(2025, 2, 1), result[0]);
        Assert.Equal(new DateOnly(2025, 2, 28), result[^1]);
        AssertAscending(result);
    }

    [Fact]
    public void GetCurrentMonth_HandlesDecemberBoundary()
    {
        var result = DateUtils.GetCurrentMonth(new DateOnly(2026, 12, 31));

        Assert.Equal(31, result.Count);
        Assert.Equal(new DateOnly(2026, 12, 1), result[0]);
        Assert.Equal(new DateOnly(2026, 12, 31), result[^1]);
        AssertAscending(result);
    }

    [Fact]
    public void FromLocalDateTime_PreservesInvalidAndAmbiguousLocalClockDates()
    {
        var timeZone = CreateEasternDaylightTimeZone();
        var invalidLocalTime = new DateTime(2024, 3, 10, 2, 30, 0, DateTimeKind.Unspecified);
        var ambiguousLocalTime = new DateTime(2024, 11, 3, 1, 30, 0, DateTimeKind.Unspecified);

        Assert.True(timeZone.IsInvalidTime(invalidLocalTime));
        Assert.True(timeZone.IsAmbiguousTime(ambiguousLocalTime));
        Assert.Equal(
            new DateOnly(2024, 3, 10),
            DateUtils.FromLocalDateTime(invalidLocalTime));
        Assert.Equal(
            new DateOnly(2024, 11, 3),
            DateUtils.FromLocalDateTime(ambiguousLocalTime));
    }

    [Fact]
    public void FromUtcInstant_DateTimeOffset_AppliesDaylightSavingAtCalendarBoundary()
    {
        var timeZone = CreateEasternDaylightTimeZone();
        var utcInstantBeforeFallTransition = new DateTimeOffset(
            2024,
            11,
            3,
            4,
            30,
            0,
            TimeSpan.Zero);
        var utcInstantAfterFallTransition = new DateTimeOffset(
            2024,
            11,
            3,
            6,
            30,
            0,
            TimeSpan.Zero);

        var dateBeforeTransition = DateUtils.FromUtcInstant(
            utcInstantBeforeFallTransition,
            timeZone);
        var dateAfterTransition = DateUtils.FromUtcInstant(
            utcInstantAfterFallTransition,
            timeZone);

        Assert.Equal(new DateOnly(2024, 11, 3), dateBeforeTransition);
        Assert.Equal(new DateOnly(2024, 11, 3), dateAfterTransition);
    }

    [Fact]
    public void FromUtcInstant_DateTime_AppliesSpringForwardRules()
    {
        var timeZone = CreateEasternDaylightTimeZone();
        var utcInstantBeforeSpringTransition = new DateTime(
            2024,
            3,
            10,
            4,
            30,
            0,
            DateTimeKind.Utc);
        var utcInstantAfterSpringTransition = new DateTime(
            2024,
            3,
            10,
            7,
            30,
            0,
            DateTimeKind.Utc);

        var dateBeforeTransition = DateUtils.FromUtcInstant(
            utcInstantBeforeSpringTransition,
            timeZone);
        var dateAfterTransition = DateUtils.FromUtcInstant(
            utcInstantAfterSpringTransition,
            timeZone);

        Assert.Equal(new DateOnly(2024, 3, 9), dateBeforeTransition);
        Assert.Equal(new DateOnly(2024, 3, 10), dateAfterTransition);
    }

    private static void AssertAscending(IReadOnlyList<DateOnly> dates)
    {
        for (var index = 1; index < dates.Count; index++)
        {
            Assert.Equal(dates[index - 1].AddDays(1), dates[index]);
        }
    }

    private static TimeZoneInfo CreateEasternDaylightTimeZone()
    {
        var daylightStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
            new DateTime(1, 1, 1, 2, 0, 0),
            3,
            2,
            DayOfWeek.Sunday);
        var daylightEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
            new DateTime(1, 1, 1, 2, 0, 0),
            11,
            1,
            DayOfWeek.Sunday);
        var adjustmentRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
            new DateTime(1, 1, 1),
            new DateTime(9999, 12, 31),
            TimeSpan.FromHours(1),
            daylightStart,
            daylightEnd);

        return TimeZoneInfo.CreateCustomTimeZone(
            "Test/Eastern",
            TimeSpan.FromHours(-5),
            "Test Eastern",
            "Test Eastern Standard",
            "Test Eastern Daylight",
            [adjustmentRule]);
    }

    private static TimeZoneInfo ResolveAvailableTimeZoneOrSkip(string timeZoneId)
    {
        try
        {
            return DateUtils.ResolveTimeZone(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            Assert.Skip($"The stable test time zone '{timeZoneId}' is unavailable.");
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            Assert.Skip($"The stable test time zone '{timeZoneId}' has invalid platform data.");
            return TimeZoneInfo.Utc;
        }
    }
}
