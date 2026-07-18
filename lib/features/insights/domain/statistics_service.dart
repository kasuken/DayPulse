import '../../../core/utilities/date_utils.dart';
import '../../check_in/domain/check_in.dart';

/// Metrics supported by statistics and trend calculations.
enum WellbeingMetric {
  /// Mood score from -5 to 5.
  mood,

  /// Energy score from 0 to 10.
  energy,

  /// Anxiety score from 0 to 10.
  anxiety,

  /// Focus score from 0 to 10.
  focus,

  /// Sleep duration in hours.
  sleep,
}

/// A chart-ready daily metric value.
final class TrendPoint {
  /// Creates a trend point for one local calendar [date].
  const TrendPoint({required this.date, required this.value});

  /// Local calendar date.
  final DateTime date;

  /// Metric value, or null when there is no entry or the metric is missing.
  final double? value;
}

/// Pure statistics over check-in domain models.
abstract final class StatisticsService {
  /// Average mood for an optional inclusive range or trailing window.
  static double? averageMood(
    List<CheckIn> entries, {
    DateTime? start,
    DateTime? end,
    int? lastDays,
    DateTime? now,
  }) => _average(
    entries,
    WellbeingMetric.mood,
    start: start,
    end: end,
    lastDays: lastDays,
    now: now,
  );

  /// Average energy for an optional inclusive range or trailing window.
  static double? averageEnergy(
    List<CheckIn> entries, {
    DateTime? start,
    DateTime? end,
    int? lastDays,
    DateTime? now,
  }) => _average(
    entries,
    WellbeingMetric.energy,
    start: start,
    end: end,
    lastDays: lastDays,
    now: now,
  );

  /// Average anxiety for an optional inclusive range or trailing window.
  static double? averageAnxiety(
    List<CheckIn> entries, {
    DateTime? start,
    DateTime? end,
    int? lastDays,
    DateTime? now,
  }) => _average(
    entries,
    WellbeingMetric.anxiety,
    start: start,
    end: end,
    lastDays: lastDays,
    now: now,
  );

  /// Average focus for an optional inclusive range or trailing window.
  static double? averageFocus(
    List<CheckIn> entries, {
    DateTime? start,
    DateTime? end,
    int? lastDays,
    DateTime? now,
  }) => _average(
    entries,
    WellbeingMetric.focus,
    start: start,
    end: end,
    lastDays: lastDays,
    now: now,
  );

  /// Average sleep hours for an optional inclusive range or trailing window.
  static double? averageSleep(
    List<CheckIn> entries, {
    DateTime? start,
    DateTime? end,
    int? lastDays,
    DateTime? now,
  }) => _average(
    entries,
    WellbeingMetric.sleep,
    start: start,
    end: end,
    lastDays: lastDays,
    now: now,
  );

  /// Number of entries in the local Monday-Sunday week containing [now].
  static int entryCountForWeek(List<CheckIn> entries, DateTime now) {
    final today = DateUtilsX.dateOnly(now);
    final start = today.subtract(
      Duration(days: today.weekday - DateTime.monday),
    );
    final end = start.add(const Duration(days: 6));
    return _entriesInRange(entries, start, end).length;
  }

  /// Number of entries in the local calendar month containing [now].
  static int entryCountForMonth(List<CheckIn> entries, DateTime now) {
    final start = DateTime(now.year, now.month);
    final end = DateTime(
      now.year,
      now.month + 1,
    ).subtract(const Duration(days: 1));
    return _entriesInRange(entries, start, end).length;
  }

  /// Consecutive entry days ending today, or yesterday when today is empty.
  static int currentStreak(List<CheckIn> entries, DateTime today) {
    final dates = _entryDates(entries);
    final dateToday = DateUtilsX.dateOnly(today);
    var cursor = dates.contains(dateToday)
        ? dateToday
        : dateToday.subtract(const Duration(days: 1));
    if (!dates.contains(cursor)) {
      return 0;
    }

    var count = 0;
    while (dates.contains(cursor)) {
      count += 1;
      cursor = cursor.subtract(const Duration(days: 1));
    }
    return count;
  }

  /// Longest consecutive-day streak in [entries].
  static int longestStreak(List<CheckIn> entries) {
    final sortedDates = _entryDates(entries).toList()..sort();
    if (sortedDates.isEmpty) {
      return 0;
    }

    var longest = 1;
    var current = 1;
    for (var i = 1; i < sortedDates.length; i += 1) {
      if (DateUtilsX.isNextDay(sortedDates[i - 1], sortedDates[i])) {
        current += 1;
      } else {
        current = 1;
      }
      if (current > longest) {
        longest = current;
      }
    }
    return longest;
  }

  /// Highest mood entry; ties resolve to the earliest entry date.
  static CheckIn? highestMoodEntry(List<CheckIn> entries) =>
      _moodExtreme(entries, highest: true);

  /// Lowest mood entry; ties resolve to the earliest entry date.
  static CheckIn? lowestMoodEntry(List<CheckIn> entries) =>
      _moodExtreme(entries, highest: false);

  /// Ordered chart data for every local day in the inclusive range.
  static List<TrendPoint> trendData(
    List<CheckIn> entries,
    WellbeingMetric metric,
    DateTime start,
    DateTime end,
  ) {
    final byDate = <DateTime, CheckIn>{};
    final sorted = entries.toList()
      ..sort(
        (a, b) => DateUtilsX.dateOnly(
          a.entryDate,
        ).compareTo(DateUtilsX.dateOnly(b.entryDate)),
      );
    for (final entry in sorted) {
      byDate.putIfAbsent(DateUtilsX.dateOnly(entry.entryDate), () => entry);
    }

    return [
      for (final date in DateUtilsX.dateRange(start, end))
        TrendPoint(date: date, value: _metricValue(byDate[date], metric)),
    ];
  }

  static double? _average(
    List<CheckIn> entries,
    WellbeingMetric metric, {
    DateTime? start,
    DateTime? end,
    int? lastDays,
    DateTime? now,
  }) {
    final range = _resolveRange(
      start: start,
      end: end,
      lastDays: lastDays,
      now: now,
    );
    final values = _entriesInRange(entries, range.$1, range.$2)
        .map((entry) => _metricValue(entry, metric))
        .whereType<double>()
        .toList(growable: false);
    if (values.isEmpty) {
      return null;
    }
    return values.reduce((a, b) => a + b) / values.length;
  }

  static (DateTime, DateTime) _resolveRange({
    DateTime? start,
    DateTime? end,
    int? lastDays,
    DateTime? now,
  }) {
    if (lastDays != null) {
      final rangeEnd = DateUtilsX.dateOnly(now ?? DateTime.now());
      final days = lastDays < 1 ? 1 : lastDays;
      return (rangeEnd.subtract(Duration(days: days - 1)), rangeEnd);
    }
    return (
      start == null ? DateTime(0) : DateUtilsX.dateOnly(start),
      end == null ? DateTime(9999, 12, 31) : DateUtilsX.dateOnly(end),
    );
  }

  static List<CheckIn> _entriesInRange(
    List<CheckIn> entries,
    DateTime start,
    DateTime end,
  ) => entries
      .where((entry) {
        final date = DateUtilsX.dateOnly(entry.entryDate);
        return !date.isBefore(DateUtilsX.dateOnly(start)) &&
            !date.isAfter(DateUtilsX.dateOnly(end));
      })
      .toList(growable: false);

  static Set<DateTime> _entryDates(List<CheckIn> entries) =>
      entries.map((entry) => DateUtilsX.dateOnly(entry.entryDate)).toSet();

  static double? _metricValue(CheckIn? entry, WellbeingMetric metric) {
    if (entry == null) {
      return null;
    }
    return switch (metric) {
      WellbeingMetric.mood => entry.mood.toDouble(),
      WellbeingMetric.energy => entry.energy?.toDouble(),
      WellbeingMetric.anxiety => entry.anxiety?.toDouble(),
      WellbeingMetric.focus => entry.focus?.toDouble(),
      WellbeingMetric.sleep => entry.sleepHours,
    };
  }

  static CheckIn? _moodExtreme(List<CheckIn> entries, {required bool highest}) {
    if (entries.isEmpty) {
      return null;
    }
    final sorted = entries.toList()
      ..sort((a, b) {
        final moodComparison = highest
            ? b.mood.compareTo(a.mood)
            : a.mood.compareTo(b.mood);
        if (moodComparison != 0) {
          return moodComparison;
        }
        return DateUtilsX.dateOnly(
          a.entryDate,
        ).compareTo(DateUtilsX.dateOnly(b.entryDate));
      });
    return sorted.first;
  }
}
