import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../app/theme/app_typography.dart';
import '../../../core/utilities/date_utils.dart';
import '../../../core/widgets/pulse_line.dart';
import '../../../l10n/app_localizations.dart';
import '../../settings/application/settings_controller.dart';
import '../application/today_controller.dart';
import '../domain/activity.dart';
import '../domain/check_in.dart';
import 'mood_labels.dart';

/// Normalizes a mood value (-5..5) to a 0..1 pulse intensity.
double _pulseIntensity(int mood) =>
    ((mood + 5) / 10).clamp(0.0, 1.0).toDouble();

/// Displays today's wellbeing status and entry actions.
class TodayScreen extends ConsumerWidget {
  /// Creates the Today screen.
  const TodayScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final locale = Localizations.localeOf(context).toString();
    final today = DateUtilsX.dateOnly(DateTime.now());
    final entry = ref.watch(todayCheckInProvider);
    final settings = ref.watch(settingsControllerProvider).value;
    final activities =
        ref.watch(todayActivitiesProvider).value ?? const <Activity>[];
    return Scaffold(
      appBar: AppBar(title: Text(l10n.todayTitle)),
      body: ListView(
        padding: const EdgeInsets.all(AppSpacing.md),
        children: [
          Text(
            l10n
                .todayFullDate(DateFormat.yMMMMEEEEd(locale).format(today))
                .toUpperCase(),
            style: AppTypography.eyebrow(context),
          ),
          const SizedBox(height: AppSpacing.lg),
          entry.when(
            data: (checkIn) => checkIn == null
                ? _EmptyTodayCard(l10n: l10n)
                : _ExistingEntryCard(
                    entry: checkIn,
                    activities: activities,
                    showEnergy: settings?.metrics.energyEnabled ?? true,
                    showAnxiety: settings?.metrics.anxietyEnabled ?? true,
                    showFocus: settings?.metrics.focusEnabled ?? true,
                    showSleep: settings?.metrics.sleepEnabled ?? true,
                  ),
            loading: () => const SizedBox.shrink(),
            error: (error, stackTrace) => Text(l10n.loadCheckInFailureMessage),
          ),
        ],
      ),
    );
  }
}

class _EmptyTodayCard extends StatelessWidget {
  const _EmptyTodayCard({required this.l10n});

  final AppLocalizations l10n;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.xl),
        child: Column(
          children: [
            const PulseLine(intensity: 0.3),
            const SizedBox(height: AppSpacing.xl),
            Text(
              l10n.todayPrompt,
              style: Theme.of(context).textTheme.headlineSmall,
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: AppSpacing.xl),
            FilledButton(
              onPressed: () => context.push('/check-in'),
              child: Text(l10n.startCheckIn),
            ),
          ],
        ),
      ),
    );
  }
}

class _ExistingEntryCard extends StatelessWidget {
  const _ExistingEntryCard({
    required this.entry,
    required this.activities,
    required this.showEnergy,
    required this.showAnxiety,
    required this.showFocus,
    required this.showSleep,
  });

  final CheckIn entry;
  final List<Activity> activities;
  final bool showEnergy;
  final bool showAnxiety;
  final bool showFocus;
  final bool showSleep;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final scheme = Theme.of(context).colorScheme;
    final activityNames = activities
        .where((activity) => entry.activityIds.contains(activity.id))
        .map((activity) => activity.name)
        .toList(growable: false);
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(AppSpacing.sm),
                  decoration: BoxDecoration(
                    color: scheme.secondaryContainer,
                    shape: BoxShape.circle,
                  ),
                  child: Text(
                    moodEmoji(entry.mood),
                    style: Theme.of(context).textTheme.headlineMedium,
                  ),
                ),
                const SizedBox(width: AppSpacing.md),
                Expanded(
                  child: Text(
                    l10n.moodValueLabel(
                      entry.mood,
                      moodLabel(l10n, entry.mood),
                    ),
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                ),
              ],
            ),
            const SizedBox(height: AppSpacing.md),
            PulseLine(intensity: _pulseIntensity(entry.mood)),
            const SizedBox(height: AppSpacing.md),
            if (showEnergy && entry.energy != null)
              _MetricRow(
                label: l10n.metricEnergy,
                value: entry.energy.toString(),
                progress: entry.energy! / 10,
              ),
            if (showAnxiety && entry.anxiety != null)
              _MetricRow(
                label: l10n.metricAnxiety,
                value: entry.anxiety.toString(),
                progress: entry.anxiety! / 10,
              ),
            if (showFocus && entry.focus != null)
              _MetricRow(
                label: l10n.metricFocus,
                value: entry.focus.toString(),
                progress: entry.focus! / 10,
              ),
            if (showSleep && entry.sleepHours != null)
              _MetricRow(
                label: l10n.metricSleep,
                value: l10n.sleepHoursValue(_formatHours(entry.sleepHours!)),
                progress: (entry.sleepHours! / 12).clamp(0.0, 1.0).toDouble(),
              ),
            if (activityNames.isNotEmpty) ...[
              const SizedBox(height: AppSpacing.md),
              Text(
                l10n.activitiesLabel.toUpperCase(),
                style: AppTypography.eyebrow(context),
              ),
              const SizedBox(height: AppSpacing.xxs),
              Text(activityNames.join(', ')),
            ],
            if ((entry.notes ?? '').isNotEmpty) ...[
              const SizedBox(height: AppSpacing.md),
              Text(
                l10n.notesPreviewLabel.toUpperCase(),
                style: AppTypography.eyebrow(context),
              ),
              const SizedBox(height: AppSpacing.xxs),
              Text(entry.notes!, maxLines: 1, overflow: TextOverflow.ellipsis),
            ],
            const SizedBox(height: AppSpacing.lg),
            FilledButton(
              onPressed: () => context.push('/check-in/${entry.id}'),
              child: Text(l10n.editCheckIn),
            ),
            const SizedBox(height: AppSpacing.sm),
            OutlinedButton(
              onPressed: () => context.push('/history/${entry.id}'),
              child: Text(l10n.viewDetails),
            ),
          ],
        ),
      ),
    );
  }

  String _formatHours(double hours) =>
      hours.toStringAsFixed(hours % 1 == 0 ? 0 : 1);
}

class _MetricRow extends StatelessWidget {
  const _MetricRow({
    required this.label,
    required this.value,
    required this.progress,
  });

  final String label;
  final String value;
  final double progress;

  @override
  Widget build(BuildContext context) {
    final scheme = Theme.of(context).colorScheme;
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.sm),
      child: Row(
        children: [
          Expanded(child: Text(label)),
          SizedBox(
            width: 88,
            child: ClipRRect(
              borderRadius: BorderRadius.circular(2),
              child: LinearProgressIndicator(
                value: progress.clamp(0.0, 1.0).toDouble(),
                minHeight: 4,
                backgroundColor: scheme.surfaceContainerHighest,
                color: scheme.primary,
              ),
            ),
          ),
          const SizedBox(width: AppSpacing.md),
          Text(value, style: AppTypography.metricValue(context)),
        ],
      ),
    );
  }
}
