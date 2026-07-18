import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../core/utilities/date_utils.dart';
import '../../../l10n/app_localizations.dart';
import '../../settings/application/settings_controller.dart';
import '../application/today_controller.dart';
import '../domain/activity.dart';
import '../domain/check_in.dart';
import 'mood_labels.dart';

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
            l10n.todayFullDate(DateFormat.yMMMMEEEEd(locale).format(today)),
            style: Theme.of(context).textTheme.titleMedium,
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
                Text(
                  moodEmoji(entry.mood),
                  style: Theme.of(context).textTheme.displaySmall,
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
            const SizedBox(height: AppSpacing.lg),
            if (showEnergy && entry.energy != null)
              _MetricRow(
                label: l10n.metricEnergy,
                value: entry.energy.toString(),
              ),
            if (showAnxiety && entry.anxiety != null)
              _MetricRow(
                label: l10n.metricAnxiety,
                value: entry.anxiety.toString(),
              ),
            if (showFocus && entry.focus != null)
              _MetricRow(
                label: l10n.metricFocus,
                value: entry.focus.toString(),
              ),
            if (showSleep && entry.sleepHours != null)
              _MetricRow(
                label: l10n.metricSleep,
                value: l10n.sleepHoursValue(_formatHours(entry.sleepHours!)),
              ),
            if (activityNames.isNotEmpty) ...[
              const SizedBox(height: AppSpacing.md),
              Text(
                l10n.activitiesLabel,
                style: Theme.of(context).textTheme.titleMedium,
              ),
              Text(activityNames.join(', ')),
            ],
            if ((entry.notes ?? '').isNotEmpty) ...[
              const SizedBox(height: AppSpacing.md),
              Text(
                l10n.notesPreviewLabel,
                style: Theme.of(context).textTheme.titleMedium,
              ),
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
  const _MetricRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.xs),
      child: Row(
        children: [
          Expanded(child: Text(label)),
          Text(value, style: Theme.of(context).textTheme.titleMedium),
        ],
      ),
    );
  }
}
