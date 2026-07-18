import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../l10n/app_localizations.dart';
import '../../check_in/domain/activity.dart';
import '../../check_in/domain/check_in.dart';
import '../../check_in/presentation/mood_labels.dart';
import '../../settings/application/settings_controller.dart';
import '../application/history_controller.dart';

/// Displays full details for one check-in entry.
class EntryDetailScreen extends ConsumerWidget {
  /// Creates the entry detail screen.
  const EntryDetailScreen({required this.id, super.key});

  /// Entry identifier.
  final String id;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final entry = ref.watch(historyEntryProvider(id));
    final activities = ref.watch(historyActivitiesProvider).value ?? const [];
    final settings = ref.watch(settingsControllerProvider).value;
    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.entryDetailsTitle),
        actions: [
          IconButton(
            tooltip: l10n.editCheckIn,
            icon: const Icon(Icons.edit_outlined),
            onPressed: () => context.push('/check-in/$id'),
          ),
          IconButton(
            tooltip: l10n.delete,
            icon: const Icon(Icons.delete_outline),
            onPressed: () => _confirmDelete(context, ref, l10n),
          ),
        ],
      ),
      body: entry.when(
        data: (checkIn) => checkIn == null
            ? Center(child: Text(l10n.checkInNotFoundMessage))
            : _EntryDetailBody(
                entry: checkIn,
                activities: activities,
                showEnergy: settings?.metrics.energyEnabled ?? true,
                showAnxiety: settings?.metrics.anxietyEnabled ?? true,
                showFocus: settings?.metrics.focusEnabled ?? true,
                showSleep: settings?.metrics.sleepEnabled ?? true,
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

  Future<void> _confirmDelete(
    BuildContext context,
    WidgetRef ref,
    AppLocalizations l10n,
  ) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.deleteCheckInTitle),
        content: Text(l10n.deleteCheckInMessage),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(l10n.cancel),
          ),
          FilledButton(
            onPressed: () => Navigator.of(context).pop(true),
            child: Text(l10n.delete),
          ),
        ],
      ),
    );
    if (confirmed != true || !context.mounted) {
      return;
    }
    await ref.read(historyControllerProvider).deleteEntry(id);
    if (context.mounted) {
      context.go('/history');
    }
  }
}

class _EntryDetailBody extends StatelessWidget {
  const _EntryDetailBody({
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
    final locale = Localizations.localeOf(context).toString();
    final activityNames = activities
        .where((activity) => entry.activityIds.contains(activity.id))
        .map((activity) => activity.name)
        .toList(growable: false);
    return ListView(
      padding: const EdgeInsets.all(AppSpacing.md),
      children: [
        Text(
          DateFormat.yMMMMEEEEd(locale).format(entry.entryDate),
          style: Theme.of(context).textTheme.headlineSmall,
        ),
        const SizedBox(height: AppSpacing.md),
        _DetailTile(
          label: l10n.metricMood,
          value:
              '${moodEmoji(entry.mood)} ${moodLabel(l10n, entry.mood)} (${_signed(entry.mood)})',
        ),
        if (showEnergy && entry.energy != null)
          _DetailTile(label: l10n.metricEnergy, value: entry.energy.toString()),
        if (showAnxiety && entry.anxiety != null)
          _DetailTile(
            label: l10n.metricAnxiety,
            value: entry.anxiety.toString(),
          ),
        if (showFocus && entry.focus != null)
          _DetailTile(label: l10n.metricFocus, value: entry.focus.toString()),
        if (showSleep && entry.sleepHours != null)
          _DetailTile(
            label: l10n.metricSleep,
            value: l10n.sleepHoursValue(_formatHours(entry.sleepHours!)),
          ),
        _DetailTile(
          label: l10n.activitiesLabel,
          value: activityNames.isEmpty
              ? l10n.noActivities
              : activityNames.join(', '),
        ),
        _DetailTile(
          label: l10n.notesLabel,
          value: (entry.notes ?? '').isEmpty ? l10n.noNotes : entry.notes!,
        ),
        _DetailTile(
          label: l10n.createdAtLabel,
          value: DateFormat.yMMMMd(locale).add_jm().format(entry.createdAt),
        ),
        _DetailTile(
          label: l10n.updatedAtLabel,
          value: DateFormat.yMMMMd(locale).add_jm().format(entry.updatedAt),
        ),
      ],
    );
  }

  String _formatHours(double hours) =>
      hours.toStringAsFixed(hours % 1 == 0 ? 0 : 1);

  String _signed(int value) => value > 0 ? '+$value' : value.toString();
}

class _DetailTile extends StatelessWidget {
  const _DetailTile({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: ListTile(title: Text(label), subtitle: Text(value)),
    );
  }
}
