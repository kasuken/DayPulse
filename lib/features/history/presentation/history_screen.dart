import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../core/widgets/pulse_line.dart';
import '../../../l10n/app_localizations.dart';
import '../../check_in/domain/activity.dart';
import '../../check_in/domain/check_in.dart';
import '../../check_in/presentation/mood_labels.dart';
import '../../settings/application/settings_controller.dart';
import '../application/history_controller.dart';

/// Displays previous check-ins in reverse chronological order.
class HistoryScreen extends ConsumerWidget {
  /// Creates the history screen.
  const HistoryScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final entries = ref.watch(historyEntriesProvider);
    final activities = ref.watch(historyActivitiesProvider).value ?? const [];
    final settings = ref.watch(settingsControllerProvider).value;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.historyTabLabel)),
      body: entries.when(
        data: (items) => items.isEmpty
            ? _EmptyHistory(l10n: l10n)
            : ListView.builder(
                padding: const EdgeInsets.all(AppSpacing.md),
                itemCount: items.length,
                itemBuilder: (context, index) => _HistoryRow(
                  entry: items[index],
                  activities: activities,
                  showEnergy: settings?.metrics.energyEnabled ?? true,
                  showAnxiety: settings?.metrics.anxietyEnabled ?? true,
                  showFocus: settings?.metrics.focusEnabled ?? true,
                ),
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

class _EmptyHistory extends StatelessWidget {
  const _EmptyHistory({required this.l10n});

  final AppLocalizations l10n;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.xl),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const PulseLine(intensity: 0.3),
            const SizedBox(height: AppSpacing.xl),
            Text(l10n.historyEmptyMessage, textAlign: TextAlign.center),
            const SizedBox(height: AppSpacing.lg),
            FilledButton(
              onPressed: () => context.push('/check-in'),
              child: Text(l10n.createFirstCheckIn),
            ),
          ],
        ),
      ),
    );
  }
}

class _HistoryRow extends StatelessWidget {
  const _HistoryRow({
    required this.entry,
    required this.activities,
    required this.showEnergy,
    required this.showAnxiety,
    required this.showFocus,
  });

  final CheckIn entry;
  final List<Activity> activities;
  final bool showEnergy;
  final bool showAnxiety;
  final bool showFocus;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final locale = Localizations.localeOf(context).toString();
    final activityNames = activities
        .where((activity) => entry.activityIds.contains(activity.id))
        .map((activity) => activity.name)
        .toList(growable: false);
    final metrics = <String>[
      if (showEnergy && entry.energy != null)
        l10n.metricShortValue(l10n.metricEnergy, entry.energy.toString()),
      if (showAnxiety && entry.anxiety != null)
        l10n.metricShortValue(l10n.metricAnxiety, entry.anxiety.toString()),
      if (showFocus && entry.focus != null)
        l10n.metricShortValue(l10n.metricFocus, entry.focus.toString()),
    ];
    return Card(
      margin: const EdgeInsets.only(bottom: AppSpacing.sm),
      child: ListTile(
        minVerticalPadding: AppSpacing.md,
        title: Text(
          DateFormat.yMMMMd(locale).format(entry.entryDate),
          style: Theme.of(
            context,
          ).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700),
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: AppSpacing.xxs),
            Text(
              '${moodEmoji(entry.mood)} ${l10n.historyMoodSigned(_signed(entry.mood))}',
            ),
            if (metrics.isNotEmpty) ...[
              const SizedBox(height: AppSpacing.xxs),
              Text(
                metrics.join(' · '),
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
            ],
            if (activityNames.isNotEmpty) ...[
              const SizedBox(height: AppSpacing.xxs),
              Text(
                activityNames.join(', '),
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
            ],
          ],
        ),
        trailing: Icon(
          Icons.chevron_right,
          color: Theme.of(context).colorScheme.onSurfaceVariant,
        ),
        onTap: () => context.push('/history/${entry.id}'),
      ),
    );
  }

  String _signed(int value) => value > 0 ? '+$value' : value.toString();
}
