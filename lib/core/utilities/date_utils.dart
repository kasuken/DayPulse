/// Date helpers for local-calendar-day logic.
///
/// DayPulse stores timestamps in UTC but treats the check-in date as a
/// local calendar date. All daily logic goes through these helpers so the
/// visible day never shifts through UTC conversion.
abstract final class DateUtilsX {
  /// Returns a date-only value (midnight, local) for [dateTime].
  static DateTime dateOnly(DateTime dateTime) =>
      DateTime(dateTime.year, dateTime.month, dateTime.day);

  /// Start of the local day containing [dateTime] (inclusive).
  static DateTime startOfDay(DateTime dateTime) => dateOnly(dateTime);

  /// End of the local day containing [dateTime] (exclusive).
  static DateTime endOfDay(DateTime dateTime) =>
      dateOnly(dateTime).add(const Duration(days: 1));

  /// Whether [a] and [b] fall on the same local calendar date.
  static bool isSameDate(DateTime a, DateTime b) =>
      a.year == b.year && a.month == b.month && a.day == b.day;

  /// Whether [b] is exactly the local calendar day after [a].
  static bool isNextDay(DateTime a, DateTime b) =>
      isSameDate(dateOnly(a).add(const Duration(days: 1)), b);

  /// Generates consecutive date-only values from [start] to [end] inclusive.
  static List<DateTime> dateRange(DateTime start, DateTime end) {
    final first = dateOnly(start);
    final last = dateOnly(end);
    if (last.isBefore(first)) {
      return const [];
    }
    final days = <DateTime>[];
    var cursor = first;
    while (!cursor.isAfter(last)) {
      days.add(cursor);
      cursor = cursor.add(const Duration(days: 1));
    }
    return days;
  }

  /// Number of whole calendar days between [a] and [b] (date-only diff).
  static int daysBetween(DateTime a, DateTime b) =>
      dateOnly(b).difference(dateOnly(a)).inDays;
}
