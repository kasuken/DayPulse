import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../core/notifications/local_notifications_reminder_service.dart';
import '../../../l10n/app_localizations.dart';
import '../../check_in/data/drift_activity_repository.dart';
import '../../check_in/data/drift_check_in_repository.dart';
import '../application/settings_controller.dart';
import '../application/settings_info_controller.dart';

/// Privacy, data deletion, and license information.
class PrivacySettingsScreen extends ConsumerWidget {
  /// Creates the privacy settings screen.
  const PrivacySettingsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final packageInfo = ref.watch(packageInfoProvider);
    return Scaffold(
      appBar: AppBar(title: Text(l10n.privacySettingsTitle)),
      body: ListView(
        padding: const EdgeInsets.all(AppSpacing.md),
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: Text(l10n.privacyLocalStorageSummary),
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          ListTile(
            enabled: false,
            leading: const Icon(Icons.file_download_outlined),
            title: Text(l10n.exportData),
            subtitle: Text(l10n.exportDataPlannedSubtitle),
            trailing: Chip(label: Text(l10n.planned)),
          ),
          ListTile(
            leading: const Icon(Icons.delete_forever_outlined),
            title: Text(l10n.deleteAllData),
            onTap: () => _confirmDeleteAll(context, ref, l10n),
          ),
          const Divider(),
          ListTile(
            leading: const Icon(Icons.info_outline),
            title: Text(l10n.appTitle),
            subtitle: packageInfo.when(
              data: (info) => Text(
                '${l10n.aboutVersion(info.version, info.buildNumber)}\n${l10n.privacyBlurb}',
              ),
              loading: () => Text(l10n.privacyBlurb),
              error: (error, stackTrace) => Text(l10n.privacyBlurb),
            ),
          ),
          ListTile(
            leading: const Icon(Icons.description_outlined),
            title: Text(l10n.openSourceLicences),
            onTap: () => showLicensePage(
              context: context,
              applicationName: l10n.appTitle,
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _confirmDeleteAll(
    BuildContext context,
    WidgetRef ref,
    AppLocalizations l10n,
  ) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.deleteAllDataTitle),
        content: Text(l10n.deleteAllDataMessage),
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
    if (confirmed != true) {
      return;
    }
    await ref.read(checkInRepositoryProvider).deleteAll();
    await ref.read(activityRepositoryProvider).resetToDefaults();
    await ref.read(reminderServiceProvider).cancelDailyReminder();
    await ref.read(settingsControllerProvider.notifier).reset();
    if (context.mounted) {
      context.go('/onboarding');
    }
  }
}
