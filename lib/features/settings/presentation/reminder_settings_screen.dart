import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../core/notifications/local_notifications_reminder_service.dart';
import '../../../l10n/app_localizations.dart';
import '../application/settings_controller.dart';
import '../domain/reminder_settings.dart';

/// Configures daily reminder notifications.
class ReminderSettingsScreen extends ConsumerWidget {
  /// Creates the reminder settings screen.
  const ReminderSettingsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final reminder =
        ref.watch(settingsControllerProvider).value?.reminder ??
        const ReminderSettings();
    final time = TimeOfDay(hour: reminder.hour, minute: reminder.minute);
    return Scaffold(
      appBar: AppBar(title: Text(l10n.reminderSettingsTitle)),
      body: ListView(
        padding: const EdgeInsets.all(AppSpacing.md),
        children: [
          SwitchListTile(
            value: reminder.enabled,
            title: Text(l10n.reminderSwitchLabel),
            onChanged: (enabled) =>
                _setEnabled(context, ref, l10n, reminder, enabled),
          ),
          ListTile(
            enabled: reminder.enabled,
            leading: const Icon(Icons.schedule_outlined),
            title: Text(l10n.reminderTimeLabel),
            subtitle: Text(time.format(context)),
            onTap: reminder.enabled
                ? () => _pickTime(context, ref, l10n, reminder)
                : null,
          ),
        ],
      ),
    );
  }

  Future<void> _setEnabled(
    BuildContext context,
    WidgetRef ref,
    AppLocalizations l10n,
    ReminderSettings reminder,
    bool enabled,
  ) async {
    final service = ref.read(reminderServiceProvider);
    if (!enabled) {
      await service.cancelDailyReminder();
      await ref
          .read(settingsControllerProvider.notifier)
          .updateReminder(reminder.copyWith(enabled: false));
      return;
    }
    final granted = await service.requestPermission();
    if (!context.mounted) {
      return;
    }
    if (!granted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(l10n.notificationPermissionSettingsHint)),
      );
      await ref
          .read(settingsControllerProvider.notifier)
          .updateReminder(reminder.copyWith(enabled: false));
      return;
    }
    final updated = reminder.copyWith(enabled: true);
    await ref.read(settingsControllerProvider.notifier).updateReminder(updated);
    try {
      await service.scheduleDailyReminder(
        TimeOfDay(hour: updated.hour, minute: updated.minute),
        title: l10n.dailyReminderTitle,
        body: l10n.dailyReminderBody,
      );
    } catch (_) {
      await ref
          .read(settingsControllerProvider.notifier)
          .updateReminder(reminder.copyWith(enabled: false));
      if (!context.mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(l10n.reminderScheduleFailed)),
      );
    }
  }

  Future<void> _pickTime(
    BuildContext context,
    WidgetRef ref,
    AppLocalizations l10n,
    ReminderSettings reminder,
  ) async {
    final picked = await showTimePicker(
      context: context,
      initialTime: TimeOfDay(hour: reminder.hour, minute: reminder.minute),
    );
    if (picked == null) {
      return;
    }
    final updated = reminder.copyWith(hour: picked.hour, minute: picked.minute);
    await ref.read(settingsControllerProvider.notifier).updateReminder(updated);
    if (updated.enabled) {
      try {
        await ref
            .read(reminderServiceProvider)
            .scheduleDailyReminder(
              picked,
              title: l10n.dailyReminderTitle,
              body: l10n.dailyReminderBody,
            );
      } catch (_) {
        await ref
            .read(settingsControllerProvider.notifier)
            .updateReminder(reminder);
        if (!context.mounted) {
          return;
        }
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l10n.reminderScheduleFailed)),
        );
      }
    }
  }
}
