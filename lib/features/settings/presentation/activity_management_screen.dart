import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:uuid/uuid.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../l10n/app_localizations.dart';
import '../../check_in/data/drift_activity_repository.dart';
import '../../check_in/domain/activity.dart';

/// Lets the user add, rename, and disable custom activities.
class ActivityManagementScreen extends ConsumerWidget {
  /// Creates the activity management screen.
  const ActivityManagementScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final activities = ref.watch(_activeActivitiesProvider);
    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.activityManagementTitle),
        actions: [
          IconButton(
            tooltip: l10n.addActivity,
            icon: const Icon(Icons.add),
            onPressed: () => _addActivity(context, ref, l10n),
          ),
        ],
      ),
      body: activities.when(
        data: (items) => ListView.builder(
          padding: const EdgeInsets.all(AppSpacing.md),
          itemCount: items.length,
          itemBuilder: (context, index) =>
              _ActivityTile(activity: items[index]),
        ),
        loading: () => const SizedBox.shrink(),
        error: (error, stackTrace) => Center(
          child: Padding(
            padding: const EdgeInsets.all(AppSpacing.lg),
            child: Text(l10n.loadCheckInFailureMessage),
          ),
        ),
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _addActivity(context, ref, l10n),
        icon: const Icon(Icons.add),
        label: Text(l10n.addActivity),
      ),
    );
  }

  Future<void> _addActivity(
    BuildContext context,
    WidgetRef ref,
    AppLocalizations l10n,
  ) async {
    final name = await showDialog<String>(
      context: context,
      builder: (context) => _ActivityNameDialog(title: l10n.newActivityTitle),
    );
    final trimmed = name?.trim();
    if (trimmed == null || trimmed.isEmpty) {
      return;
    }
    final repository = ref.read(activityRepositoryProvider);
    final active = await repository.getActive();
    final nextSortOrder = active.isEmpty
        ? 0
        : active
                  .map((activity) => activity.sortOrder)
                  .reduce((a, b) => a > b ? a : b) +
              1;
    await repository.add(
      Activity(
        id: const Uuid().v4(),
        name: trimmed,
        isDefault: false,
        isActive: true,
        sortOrder: nextSortOrder,
      ),
    );
  }
}

final _activeActivitiesProvider = StreamProvider.autoDispose<List<Activity>>((
  ref,
) {
  return ref.watch(activityRepositoryProvider).watchActive();
});

class _ActivityTile extends ConsumerWidget {
  const _ActivityTile({required this.activity});

  final Activity activity;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    return Card(
      child: ListTile(
        leading: Icon(
          activity.isDefault ? Icons.lock_outline : Icons.label_outline,
        ),
        title: Text(activity.name),
        subtitle: activity.isDefault ? Text(l10n.activityDefaultLocked) : null,
        trailing: activity.isDefault
            ? null
            : PopupMenuButton<_ActivityAction>(
                tooltip: l10n.activityManagementTitle,
                onSelected: (action) =>
                    _handleAction(context, ref, l10n, action),
                itemBuilder: (context) => [
                  PopupMenuItem<_ActivityAction>(
                    value: _ActivityAction.rename,
                    child: Text(l10n.rename),
                  ),
                  PopupMenuItem<_ActivityAction>(
                    value: _ActivityAction.disable,
                    child: Text(l10n.disable),
                  ),
                ],
              ),
      ),
    );
  }

  Future<void> _handleAction(
    BuildContext context,
    WidgetRef ref,
    AppLocalizations l10n,
    _ActivityAction action,
  ) async {
    switch (action) {
      case _ActivityAction.rename:
        await _rename(context, ref, l10n);
      case _ActivityAction.disable:
        await _disable(context, ref, l10n);
    }
  }

  Future<void> _rename(
    BuildContext context,
    WidgetRef ref,
    AppLocalizations l10n,
  ) async {
    final name = await showDialog<String>(
      context: context,
      builder: (context) => _ActivityNameDialog(
        title: l10n.renameActivityTitle,
        initialValue: activity.name,
      ),
    );
    final trimmed = name?.trim();
    if (trimmed == null || trimmed.isEmpty) {
      return;
    }
    await ref
        .read(activityRepositoryProvider)
        .update(activity.copyWith(name: trimmed));
  }

  Future<void> _disable(
    BuildContext context,
    WidgetRef ref,
    AppLocalizations l10n,
  ) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.disableActivityTitle),
        content: Text(l10n.disableActivityMessage),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(l10n.cancel),
          ),
          FilledButton(
            style: FilledButton.styleFrom(
              backgroundColor: Theme.of(context).colorScheme.error,
              foregroundColor: Theme.of(context).colorScheme.onError,
            ),
            onPressed: () => Navigator.of(context).pop(true),
            child: Text(l10n.disable),
          ),
        ],
      ),
    );
    if (confirmed == true) {
      await ref.read(activityRepositoryProvider).deactivate(activity.id);
    }
  }
}

enum _ActivityAction { rename, disable }

class _ActivityNameDialog extends StatefulWidget {
  const _ActivityNameDialog({required this.title, this.initialValue});

  final String title;
  final String? initialValue;

  @override
  State<_ActivityNameDialog> createState() => _ActivityNameDialogState();
}

class _ActivityNameDialogState extends State<_ActivityNameDialog> {
  late final TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialValue ?? '');
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return AlertDialog(
      title: Text(widget.title),
      content: TextField(
        controller: _controller,
        autofocus: true,
        textCapitalization: TextCapitalization.sentences,
        decoration: InputDecoration(labelText: l10n.activityNameLabel),
        onSubmitted: (value) => Navigator.of(context).pop(value),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text(l10n.cancel),
        ),
        FilledButton(
          onPressed: () => Navigator.of(context).pop(_controller.text),
          child: Text(widget.initialValue == null ? l10n.add : l10n.rename),
        ),
      ],
    );
  }
}
