using System.Globalization;

namespace DayPulse.App.Shared;

/// <summary>
/// Formats user-facing dates, times, and numbers with the active UI culture.
/// </summary>
public sealed class LocalizedFormatService
{
    /// <summary>
    /// Formats a domain calendar date without changing its <see cref="DateOnly"/> semantics.
    /// </summary>
    /// <param name="date">The calendar date to format.</param>
    /// <param name="format">The culture-sensitive date format.</param>
    /// <returns>The formatted date.</returns>
    public string FormatDate(DateOnly date, string? format = "d")
    {
        return date.ToString(format, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Formats a local date and time with the active UI culture.
    /// </summary>
    /// <param name="dateTime">The date and time to format.</param>
    /// <param name="format">The culture-sensitive date and time format.</param>
    /// <returns>The formatted date and time.</returns>
    public string FormatDateTime(DateTime dateTime, string? format = "g")
    {
        return dateTime.ToString(format, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Formats an offset date and time with the active UI culture.
    /// </summary>
    /// <param name="dateTime">The offset date and time to format.</param>
    /// <param name="format">The culture-sensitive date and time format.</param>
    /// <returns>The formatted date and time.</returns>
    public string FormatDateTime(DateTimeOffset dateTime, string? format = "g")
    {
        return dateTime.ToString(format, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Formats a number with the active UI culture.
    /// </summary>
    /// <typeparam name="T">The formattable numeric type.</typeparam>
    /// <param name="value">The number to format.</param>
    /// <param name="format">The culture-sensitive numeric format.</param>
    /// <returns>The formatted number.</returns>
    public string FormatNumber<T>(T value, string? format = "N")
        where T : IFormattable
    {
        return value.ToString(format, CultureInfo.CurrentCulture);
    }
}
