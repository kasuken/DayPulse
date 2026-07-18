import 'package:daypulse/features/check_in/domain/check_in.dart';
import 'package:daypulse/features/insights/domain/statistics_service.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('StatisticsService', () {
    test('returns null averages for empty or missing metrics', () {
      expect(StatisticsService.averageMood(const []), isNull);
      expect(
        StatisticsService.averageEnergy([_entry(day: 1, mood: 1)]),
        isNull,
      );
    });

    test('averages over 7 and 30 day trailing windows', () {
      final entries = [
        _entry(day: 1, mood: 5, energy: 4),
        _entry(day: 25, mood: 1, energy: 2),
        _entry(day: 30, mood: 3, energy: 8),
      ];

      expect(
        StatisticsService.averageMood(
          entries,
          lastDays: 7,
          now: DateTime(2024, 1, 31),
        ),
        2,
      );
      expect(
        StatisticsService.averageEnergy(
          entries,
          lastDays: 30,
          now: DateTime(2024, 1, 31),
        ),
        closeTo(14 / 3, 0.001),
      );
    });

    test('counts local week and month entries', () {
      final entries = [
        _entry(day: 29, mood: 1),
        _entry(day: 31, mood: 1),
        _entry(month: 2, day: 1, mood: 1),
      ];

      expect(
        StatisticsService.entryCountForWeek(entries, DateTime(2024, 1, 31)),
        3,
      );
      expect(
        StatisticsService.entryCountForMonth(entries, DateTime(2024, 1, 31)),
        2,
      );
    });

    test('currentStreak ends today or uses yesterday grace rule', () {
      final entries = [
        _entry(day: 8, mood: 1),
        _entry(day: 9, mood: 1),
        _entry(day: 10, mood: 1),
      ];

      expect(
        StatisticsService.currentStreak(entries, DateTime(2024, 1, 10)),
        3,
      );
      expect(
        StatisticsService.currentStreak(entries, DateTime(2024, 1, 11)),
        3,
      );
      expect(
        StatisticsService.currentStreak(entries, DateTime(2024, 1, 12)),
        0,
      );
    });

    test('longestStreak ignores duplicate dates', () {
      final entries = [
        _entry(day: 1, mood: 1, id: 'a'),
        _entry(day: 1, mood: 2, id: 'b'),
        _entry(day: 2, mood: 1),
        _entry(day: 4, mood: 1),
        _entry(day: 5, mood: 1),
        _entry(day: 6, mood: 1),
      ];

      expect(StatisticsService.longestStreak(entries), 3);
    });

    test('highest and lowest mood ties resolve to earliest date', () {
      final entries = [
        _entry(day: 2, mood: 5, id: 'late-high'),
        _entry(day: 1, mood: 5, id: 'early-high'),
        _entry(day: 4, mood: -3, id: 'late-low'),
        _entry(day: 3, mood: -3, id: 'early-low'),
      ];

      expect(StatisticsService.highestMoodEntry(entries)?.id, 'early-high');
      expect(StatisticsService.lowestMoodEntry(entries)?.id, 'early-low');
    });

    test('trendData includes every date once with nulls for gaps', () {
      final entries = [
        _entry(day: 1, mood: 1, focus: 7),
        _entry(day: 3, mood: 3),
      ];

      final trend = StatisticsService.trendData(
        entries,
        WellbeingMetric.focus,
        DateTime(2024, 1, 1),
        DateTime(2024, 1, 3),
      );

      expect(trend.map((point) => point.date), [
        DateTime(2024, 1, 1),
        DateTime(2024, 1, 2),
        DateTime(2024, 1, 3),
      ]);
      expect(trend.map((point) => point.value), [7, null, null]);
    });
  });
}

CheckIn _entry({
  int month = 1,
  required int day,
  required int mood,
  String? id,
  int? energy,
  int? anxiety,
  int? focus,
  double? sleepHours,
}) {
  final date = DateTime(2024, month, day);
  return CheckIn(
    id: id ?? 'entry-$month-$day-$mood',
    entryDate: date,
    mood: mood,
    energy: energy,
    anxiety: anxiety,
    focus: focus,
    sleepHours: sleepHours,
    activityIds: const [],
    createdAt: date.toUtc(),
    updatedAt: date.toUtc(),
  );
}
