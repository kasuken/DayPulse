import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/utilities/date_utils.dart';
import '../../check_in/data/drift_check_in_repository.dart';
import '../../check_in/domain/check_in.dart';
import '../../settings/application/settings_controller.dart';
import '../../settings/domain/wellbeing_settings.dart';
import '../domain/statistics_service.dart';

/// Selected period length for insights charts.
final insightsPeriodProvider =
    NotifierProvider.autoDispose<InsightsPeriodController, int>(
      InsightsPeriodController.new,
    );

/// Controls the selected insights period.
class InsightsPeriodController extends Notifier<int> {
  @override
  int build() => 7;

  /// Selects the period length in days.
  void select(int days) {
    state = days;
  }
}

/// Streams check-ins for Insights.
final insightsEntriesProvider = StreamProvider.autoDispose<List<CheckIn>>((
  ref,
) {
  return ref.watch(checkInRepositoryProvider).watchAll();
});

/// Computes summary and trend data for Insights.
final insightsSnapshotProvider =
    Provider.autoDispose<AsyncValue<InsightsSnapshot>>((ref) {
      final entries = ref.watch(insightsEntriesProvider);
      final settings = ref.watch(settingsControllerProvider);
      final periodDays = ref.watch(insightsPeriodProvider);
      return entries.whenData((loadedEntries) {
        final metrics = settings.value?.metrics ?? const WellbeingSettings();
        return InsightsSnapshot.fromEntries(
          entries: loadedEntries,
          metrics: metrics,
          periodDays: periodDays,
          now: DateTime.now(),
        );
      });
    });

/// Insights data prepared for presentation.
class InsightsSnapshot {
  /// Creates an insights snapshot.
  const InsightsSnapshot({
    required this.metrics,
    required this.entriesThisMonth,
    required this.currentStreak,
    required this.longestStreak,
    required this.highestMoodEntry,
    required this.lowestMoodEntry,
    required this.averages,
    required this.trends,
    required this.periodDays,
  });

  /// Builds a snapshot from check-in entries.
  factory InsightsSnapshot.fromEntries({
    required List<CheckIn> entries,
    required WellbeingSettings metrics,
    required int periodDays,
    required DateTime now,
  }) {
    final today = DateUtilsX.dateOnly(now);
    final start = today.subtract(Duration(days: periodDays - 1));
    return InsightsSnapshot(
      metrics: metrics,
      entriesThisMonth: StatisticsService.entryCountForMonth(entries, today),
      currentStreak: StatisticsService.currentStreak(entries, today),
      longestStreak: StatisticsService.longestStreak(entries),
      highestMoodEntry: StatisticsService.highestMoodEntry(entries),
      lowestMoodEntry: StatisticsService.lowestMoodEntry(entries),
      averages: {
        WellbeingMetric.mood: StatisticsService.averageMood(
          entries,
          lastDays: 7,
          now: today,
        ),
        WellbeingMetric.energy: StatisticsService.averageEnergy(
          entries,
          lastDays: 7,
          now: today,
        ),
        WellbeingMetric.anxiety: StatisticsService.averageAnxiety(
          entries,
          lastDays: 7,
          now: today,
        ),
        WellbeingMetric.focus: StatisticsService.averageFocus(
          entries,
          lastDays: 7,
          now: today,
        ),
        WellbeingMetric.sleep: StatisticsService.averageSleep(
          entries,
          lastDays: 7,
          now: today,
        ),
      },
      trends: {
        for (final metric in WellbeingMetric.values)
          metric: StatisticsService.trendData(entries, metric, start, today),
      },
      periodDays: periodDays,
    );
  }

  /// Enabled optional metric settings.
  final WellbeingSettings metrics;

  /// Number of entries in the current month.
  final int entriesThisMonth;

  /// Current consecutive check-in streak.
  final int currentStreak;

  /// Longest check-in streak.
  final int longestStreak;

  /// Best mood entry.
  final CheckIn? highestMoodEntry;

  /// Lowest mood entry.
  final CheckIn? lowestMoodEntry;

  /// Seven-day averages by metric.
  final Map<WellbeingMetric, double?> averages;

  /// Period trend points by metric.
  final Map<WellbeingMetric, List<TrendPoint>> trends;

  /// Selected trend period in days.
  final int periodDays;
}
