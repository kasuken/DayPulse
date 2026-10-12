namespace DayPulse.Core;

/// <summary>
/// Provides calendar-date and time-zone conversion helpers for the domain.
/// </summary>
public static class DateUtils
{
    private const int Past7DaysCount = 7;
    private const int Past30DaysCount = 30;
    private const int Past90DaysCount = 90;

    /// <summary>
    /// Gets the calendar date represented by a local <see cref="DateTime"/>.
    /// </summary>
    /// <remarks>
    /// The date and time fields are preserved as supplied; this method does not convert
    /// the value through UTC. Use <see cref="FromUtcInstant(DateTime, TimeZoneInfo)"/>
    /// when the value is a UTC instant.
    /// </remarks>
    /// <param name="localDateTime">
    /// A local or unspecified <see cref="DateTime"/> whose calendar fields represent the
    /// desired local date.
    /// </param>
    /// <returns>The local calendar date represented by <paramref name="localDateTime"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="localDateTime"/> has <see cref="DateTimeKind.Utc"/>.
    /// </exception>
    public static DateOnly FromLocalDateTime(DateTime localDateTime)
    {
        if (localDateTime.Kind == DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "A UTC DateTime must be converted with FromUtcInstant and an explicit time zone.",
                nameof(localDateTime));
        }

        return DateOnly.FromDateTime(localDateTime);
    }

    /// <summary>
    /// Gets the calendar date represented by a local <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <remarks>
    /// The local date and time fields represented by the offset are preserved; the value
    /// is not converted to UTC or to another time zone. This is appropriate for a
    /// user-facing local value such as a check-in entry date.
    /// </remarks>
    /// <param name="localDateTime">
    /// A <see cref="DateTimeOffset"/> whose local clock fields represent the desired date.
    /// </param>
    /// <returns>The local calendar date represented by <paramref name="localDateTime"/>.</returns>
    public static DateOnly FromLocalDateTime(DateTimeOffset localDateTime)
    {
        return DateOnly.FromDateTime(localDateTime.Date);
    }

    /// <summary>
    /// Converts a UTC <see cref="DateTime"/> instant to the calendar date in a time zone.
    /// </summary>
    /// <remarks>
    /// The conversion is performed by <see cref="TimeZoneInfo"/> from the instant, so
    /// daylight-saving transitions are applied by the supplied time zone. A UTC instant
    /// has no ambiguous local interpretation.
    /// </remarks>
    /// <param name="utcInstant">A <see cref="DateTime"/> whose <see cref="DateTime.Kind"/> is UTC.</param>
    /// <param name="timeZone">The destination time zone.</param>
    /// <returns>The destination time zone's local calendar date.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="utcInstant"/> is not marked as UTC.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeZone"/> is <see langword="null"/>.</exception>
    public static DateOnly FromUtcInstant(DateTime utcInstant, TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        if (utcInstant.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "The UTC instant must have DateTimeKind.Utc.",
                nameof(utcInstant));
        }

        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcInstant, timeZone);
        return DateOnly.FromDateTime(localDateTime);
    }

    /// <summary>
    /// Converts a UTC <see cref="DateTimeOffset"/> instant to the calendar date in a time zone.
    /// </summary>
    /// <remarks>
    /// A <see cref="DateTimeOffset"/> identifies an instant regardless of its stored offset.
    /// The conversion uses the supplied time zone's rules, including daylight-saving
    /// transitions, and then takes the resulting local calendar date.
    /// </remarks>
    /// <param name="utcInstant">The instant to convert. Its offset may be any valid offset.</param>
    /// <param name="timeZone">The destination time zone.</param>
    /// <returns>The destination time zone's local calendar date.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeZone"/> is <see langword="null"/>.</exception>
    public static DateOnly FromUtcInstant(DateTimeOffset utcInstant, TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        var localDateTime = TimeZoneInfo.ConvertTime(utcInstant, timeZone);
        return DateOnly.FromDateTime(localDateTime.Date);
    }

    /// <summary>
    /// Resolves a system time zone from either an IANA or Windows time zone identifier.
    /// </summary>
    /// <remarks>
    /// On .NET 10, <see cref="TimeZoneInfo.FindSystemTimeZoneById(string)"/> accepts both
    /// identifier formats and converts between them when the platform has the required
    /// time-zone data. Keeping the identifier at the boundary and passing the resolved
    /// <see cref="TimeZoneInfo"/> to conversion methods keeps the conversion path
    /// platform-neutral.
    /// </remarks>
    /// <param name="timeZoneId">An IANA or Windows system time-zone identifier.</param>
    /// <returns>The resolved system time zone.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeZoneId"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="timeZoneId"/> is empty or whitespace.</exception>
    /// <exception cref="TimeZoneNotFoundException">
    /// Thrown when the identifier is not available on the current platform.
    /// </exception>
    /// <exception cref="InvalidTimeZoneException">
    /// Thrown when the platform's time-zone data for the identifier is invalid.
    /// </exception>
    public static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    /// <summary>
    /// Determines whether <paramref name="laterDate"/> is exactly one calendar day after
    /// <paramref name="earlierDate"/>.
    /// </summary>
    /// <remarks>
    /// The arguments are ordered: the first date must be earlier than the second date.
    /// Equal dates and reversed dates return <see langword="false"/>. The comparison uses
    /// <see cref="DateOnly.DayNumber"/>, not elapsed timestamp hours, so month ends,
    /// year ends, and leap days are handled as calendar boundaries.
    /// </remarks>
    /// <param name="earlierDate">The expected earlier calendar date.</param>
    /// <param name="laterDate">The expected immediately following calendar date.</param>
    /// <returns>
    /// <see langword="true"/> when the dates are ordered and consecutive; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool AreConsecutiveDays(DateOnly earlierDate, DateOnly laterDate)
    {
        return laterDate > earlierDate
            && laterDate.DayNumber - earlierDate.DayNumber == 1;
    }

    /// <summary>
    /// Gets the past seven calendar days in ascending order, including
    /// <paramref name="currentDate"/>.
    /// </summary>
    /// <param name="currentDate">The inclusive final date of the range.</param>
    /// <returns>
    /// Seven dates from <c>currentDate.AddDays(-6)</c> through
    /// <paramref name="currentDate"/>, inclusive.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the range would begin before <see cref="DateOnly.MinValue"/>.
    /// </exception>
    public static IReadOnlyList<DateOnly> GetPast7Days(DateOnly currentDate)
    {
        return GetPastDays(currentDate, Past7DaysCount);
    }

    /// <summary>
    /// Gets the past 30 calendar days in ascending order, including
    /// <paramref name="currentDate"/>.
    /// </summary>
    /// <param name="currentDate">The inclusive final date of the range.</param>
    /// <returns>
    /// Thirty dates from <c>currentDate.AddDays(-29)</c> through
    /// <paramref name="currentDate"/>, inclusive.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the range would begin before <see cref="DateOnly.MinValue"/>.
    /// </exception>
    public static IReadOnlyList<DateOnly> GetPast30Days(DateOnly currentDate)
    {
        return GetPastDays(currentDate, Past30DaysCount);
    }

    /// <summary>
    /// Gets the past 90 calendar days in ascending order, including
    /// <paramref name="currentDate"/>.
    /// </summary>
    /// <param name="currentDate">The inclusive final date of the range.</param>
    /// <returns>
    /// Ninety dates from <c>currentDate.AddDays(-89)</c> through
    /// <paramref name="currentDate"/>, inclusive.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the range would begin before <see cref="DateOnly.MinValue"/>.
    /// </exception>
    public static IReadOnlyList<DateOnly> GetPast90Days(DateOnly currentDate)
    {
        return GetPastDays(currentDate, Past90DaysCount);
    }

    /// <summary>
    /// Gets the calendar week containing <paramref name="date"/> in ascending order.
    /// </summary>
    /// <remarks>
    /// The default week starts on Monday. Pass a different
    /// <paramref name="firstDayOfWeek"/> when the user's calendar uses another
    /// convention. The returned range always contains exactly seven dates, including
    /// both week boundaries.
    /// </remarks>
    /// <param name="date">Any date in the week to return.</param>
    /// <param name="firstDayOfWeek">The inclusive first day of the calendar week.</param>
    /// <returns>
    /// Seven dates from the selected first day through the day before the next selected
    /// first day, in ascending order.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="firstDayOfWeek"/> is not a defined
    /// <see cref="DayOfWeek"/> value, or when the seven-day range would extend beyond
    /// the representable <see cref="DateOnly"/> range.
    /// </exception>
    public static IReadOnlyList<DateOnly> GetCurrentWeek(
        DateOnly date,
        DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
    {
        if (firstDayOfWeek is < DayOfWeek.Sunday or > DayOfWeek.Saturday)
        {
            throw new ArgumentOutOfRangeException(
                nameof(firstDayOfWeek),
                firstDayOfWeek,
                "The first day of the week must be a defined DayOfWeek value.");
        }

        var daysFromStart = ((int)date.DayOfWeek - (int)firstDayOfWeek + 7) % 7;
        var startDayNumber = date.DayNumber - daysFromStart;
        const int weekLength = 7;
        if (startDayNumber < DateOnly.MinValue.DayNumber
            || startDayNumber > DateOnly.MaxValue.DayNumber - (weekLength - 1))
        {
            throw new ArgumentOutOfRangeException(
                nameof(date),
                date,
                "The calendar week would extend beyond the representable DateOnly range.");
        }

        var startDate = DateOnly.FromDayNumber(startDayNumber);
        return CreateAscendingRange(startDate, 7);
    }

    /// <summary>
    /// Gets every date in the calendar month containing <paramref name="date"/> in
    /// ascending order.
    /// </summary>
    /// <param name="date">Any date in the month to return.</param>
    /// <returns>
    /// A range from the first through the last day of the month, inclusive.
    /// </returns>
    public static IReadOnlyList<DateOnly> GetCurrentMonth(DateOnly date)
    {
        var startDate = new DateOnly(date.Year, date.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        return CreateAscendingRange(startDate, daysInMonth);
    }

    private static IReadOnlyList<DateOnly> GetPastDays(DateOnly currentDate, int count)
    {
        var startDayNumber = currentDate.DayNumber - (count - 1);
        if (startDayNumber < DateOnly.MinValue.DayNumber)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentDate),
                currentDate,
                "The requested calendar range would begin before DateOnly.MinValue.");
        }

        var startDate = DateOnly.FromDayNumber(startDayNumber);
        return CreateAscendingRange(startDate, count);
    }

    private static IReadOnlyList<DateOnly> CreateAscendingRange(DateOnly startDate, int count)
    {
        var dates = new DateOnly[count];

        for (var index = 0; index < count; index++)
        {
            dates[index] = startDate.AddDays(index);
        }

        return Array.AsReadOnly(dates);
    }
}
