import 'package:fl_chart/fl_chart.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../l10n/app_localizations.dart';
import '../application/insights_controller.dart';
import '../domain/statistics_service.dart';

/// Shows wellbeing summaries and trend charts.
class InsightsScreen extends ConsumerWidget {
  /// Creates the insights screen.
  const InsightsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final snapshot = ref.watch(insightsSnapshotProvider);
    final period = ref.watch(insightsPeriodProvider);
    return Scaffold(
      appBar: AppBar(title: Text(l10n.insightsTabLabel)),
      body: snapshot.when(
        data: (data) => ListView(
          padding: const EdgeInsets.all(AppSpacing.md),
          children: [
            SegmentedButton<int>(
              segments: [
                ButtonSegment(value: 7, label: Text(l10n.insightsPeriod7Days)),
                ButtonSegment(
                  value: 30,
                  label: Text(l10n.insightsPeriod30Days),
                ),
                ButtonSegment(
                  value: 90,
                  label: Text(l10n.insightsPeriod90Days),
                ),
              ],
              selected: {period},
              onSelectionChanged: (values) => ref
                  .read(insightsPeriodProvider.notifier)
                  .select(values.first),
            ),
            const SizedBox(height: AppSpacing.lg),
            _SummaryGrid(snapshot: data),
            const SizedBox(height: AppSpacing.lg),
            for (final metric in _enabledMetrics(data))
              _TrendCard(
                title: l10n.trendChartTitle(_metricLabel(l10n, metric)),
                semanticsLabel: l10n.trendChartSemantics(
                  _metricLabel(l10n, metric),
                ),
                points: data.trends[metric] ?? const <TrendPoint>[],
                metric: metric,
              ),
          ],
        ),
        loading: () => const SizedBox.shrink(),
        error: (error, stackTrace) => Center(
          child: Padding(
            padding: const EdgeInsets.all(AppSpacing.lg),
            child: Text(l10n.loadCheckInFailureMessage),
          ),
        ),
      ),
    );
  }
}

class _SummaryGrid extends StatelessWidget {
  const _SummaryGrid({required this.snapshot});

  final InsightsSnapshot snapshot;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final locale = Localizations.localeOf(context).toString();
    final cards = <_SummaryCardData>[
      for (final metric in _enabledMetrics(snapshot))
        _SummaryCardData(
          l10n.averageMetricTitle(_metricLabel(l10n, metric)),
          _formatAverage(l10n, snapshot.averages[metric], metric),
        ),
      _SummaryCardData(
        l10n.entriesThisMonth,
        l10n.entriesCount(snapshot.entriesThisMonth),
      ),
      _SummaryCardData(
        l10n.currentStreak,
        l10n.daysCount(snapshot.currentStreak),
      ),
      _SummaryCardData(
        l10n.longestStreak,
        l10n.daysCount(snapshot.longestStreak),
      ),
      _SummaryCardData(
        l10n.bestMoodDay,
        snapshot.highestMoodEntry == null
            ? l10n.notAvailable
            : DateFormat.yMMMd(
                locale,
              ).format(snapshot.highestMoodEntry!.entryDate),
      ),
      _SummaryCardData(
        l10n.lowestMoodDay,
        snapshot.lowestMoodEntry == null
            ? l10n.notAvailable
            : DateFormat.yMMMd(
                locale,
              ).format(snapshot.lowestMoodEntry!.entryDate),
      ),
    ];
    return GridView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      gridDelegate: const SliverGridDelegateWithMaxCrossAxisExtent(
        maxCrossAxisExtent: 220,
        mainAxisSpacing: AppSpacing.sm,
        crossAxisSpacing: AppSpacing.sm,
        childAspectRatio: 1.35,
      ),
      itemCount: cards.length,
      itemBuilder: (context, index) => _SummaryCard(data: cards[index]),
    );
  }
}

class _SummaryCardData {
  const _SummaryCardData(this.title, this.value);

  final String title;
  final String value;
}

class _SummaryCard extends StatelessWidget {
  const _SummaryCard({required this.data});

  final _SummaryCardData data;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(data.title, style: Theme.of(context).textTheme.labelLarge),
            const SizedBox(height: AppSpacing.sm),
            Text(data.value, style: Theme.of(context).textTheme.headlineSmall),
          ],
        ),
      ),
    );
  }
}

class _TrendCard extends StatelessWidget {
  const _TrendCard({
    required this.title,
    required this.semanticsLabel,
    required this.points,
    required this.metric,
  });

  final String title;
  final String semanticsLabel;
  final List<TrendPoint> points;
  final WellbeingMetric metric;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final valuedPoints = points
        .where((point) => point.value != null)
        .toList(growable: false);
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title, style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: AppSpacing.md),
            if (valuedPoints.length < 2)
              Text(l10n.insufficientTrendData)
            else
              Semantics(
                label: semanticsLabel,
                child: SizedBox(
                  height: 220,
                  child: LineChart(_chartData(context, points, metric)),
                ),
              ),
          ],
        ),
      ),
    );
  }

  LineChartData _chartData(
    BuildContext context,
    List<TrendPoint> points,
    WellbeingMetric metric,
  ) {
    final range = _range(metric);
    return LineChartData(
      minX: 0,
      maxX: (points.length - 1).toDouble(),
      minY: range.$1,
      maxY: range.$2,
      lineTouchData: const LineTouchData(enabled: true),
      gridData: const FlGridData(show: true),
      titlesData: FlTitlesData(
        topTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
        rightTitles: const AxisTitles(
          sideTitles: SideTitles(showTitles: false),
        ),
        leftTitles: AxisTitles(
          sideTitles: SideTitles(
            showTitles: true,
            reservedSize: 36,
            interval: _interval(metric),
          ),
        ),
        bottomTitles: AxisTitles(
          sideTitles: SideTitles(
            showTitles: true,
            reservedSize: 30,
            interval: (points.length / 3).ceilToDouble(),
            getTitlesWidget: (value, meta) {
              final index = value.round();
              if (index < 0 || index >= points.length) {
                return const SizedBox.shrink();
              }
              final locale = Localizations.localeOf(context).toString();
              return Text(
                DateFormat.Md(locale).format(points[index].date),
                style: Theme.of(context).textTheme.labelSmall,
              );
            },
          ),
        ),
      ),
      lineBarsData: _segments(points)
          .map(
            (spots) => LineChartBarData(
              spots: spots,
              isCurved: false,
              dotData: const FlDotData(show: true),
              barWidth: 3,
              color: Theme.of(context).colorScheme.primary,
            ),
          )
          .toList(growable: false),
    );
  }

  List<List<FlSpot>> _segments(List<TrendPoint> points) {
    final segments = <List<FlSpot>>[];
    var current = <FlSpot>[];
    for (final (index, point) in points.indexed) {
      final value = point.value;
      if (value == null) {
        if (current.isNotEmpty) {
          segments.add(current);
          current = <FlSpot>[];
        }
      } else {
        current.add(FlSpot(index.toDouble(), value));
      }
    }
    if (current.isNotEmpty) {
      segments.add(current);
    }
    return segments;
  }

  (double, double) _range(WellbeingMetric metric) => switch (metric) {
    WellbeingMetric.mood => (-5, 5),
    WellbeingMetric.sleep => (0, 15),
    WellbeingMetric.energy ||
    WellbeingMetric.anxiety ||
    WellbeingMetric.focus => (0, 10),
  };

  double _interval(WellbeingMetric metric) =>
      metric == WellbeingMetric.sleep ? 5 : 2.5;
}

List<WellbeingMetric> _enabledMetrics(InsightsSnapshot snapshot) => [
  WellbeingMetric.mood,
  if (snapshot.metrics.energyEnabled) WellbeingMetric.energy,
  if (snapshot.metrics.anxietyEnabled) WellbeingMetric.anxiety,
  if (snapshot.metrics.focusEnabled) WellbeingMetric.focus,
  if (snapshot.metrics.sleepEnabled) WellbeingMetric.sleep,
];

String _metricLabel(AppLocalizations l10n, WellbeingMetric metric) =>
    switch (metric) {
      WellbeingMetric.mood => l10n.metricMood,
      WellbeingMetric.energy => l10n.metricEnergy,
      WellbeingMetric.anxiety => l10n.metricAnxiety,
      WellbeingMetric.focus => l10n.metricFocus,
      WellbeingMetric.sleep => l10n.metricSleep,
    };

String _formatAverage(
  AppLocalizations l10n,
  double? value,
  WellbeingMetric metric,
) {
  if (value == null) {
    return '—';
  }
  final formatted = value.toStringAsFixed(
    metric == WellbeingMetric.sleep ? 1 : 0,
  );
  return metric == WellbeingMetric.sleep
      ? l10n.sleepHoursShort(formatted)
      : formatted;
}
