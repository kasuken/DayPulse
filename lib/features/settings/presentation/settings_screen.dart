import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../l10n/app_localizations.dart';
import '../application/settings_controller.dart';
import '../application/settings_info_controller.dart';
import '../domain/wellbeing_settings.dart';

/// Settings hub for preferences and app information.
class SettingsScreen extends ConsumerWidget {
  /// Creates the settings screen.
  const SettingsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final settings = ref.watch(settingsControllerProvider).value;
    final metrics = settings?.metrics ?? const WellbeingSettings();
    return Scaffold(
      appBar: AppBar(title: Text(l10n.settingsTabLabel)),
      body: ListView(
        padding: const EdgeInsets.all(AppSpacing.md),
        children: [
          ListTile(
            leading: const Icon(Icons.palette_outlined),
            title: Text(l10n.appearanceSettingsTitle),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.push('/settings/appearance'),
          ),
          const Divider(),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: AppSpacing.md),
            child: Text(
              l10n.settingsMetricsTitle,
              style: Theme.of(context).textTheme.titleMedium,
            ),
          ),
          SwitchListTile(
            value: true,
            onChanged: null,
            title: Text(l10n.metricMood),
            subtitle: Text(l10n.settingsMoodAlwaysOn),
          ),
          _MetricSwitch(
            title: l10n.metricEnergy,
            value: metrics.energyEnabled,
            update: (value) => metrics.copyWith(energyEnabled: value),
          ),
          _MetricSwitch(
            title: l10n.metricAnxiety,
            value: metrics.anxietyEnabled,
            update: (value) => metrics.copyWith(anxietyEnabled: value),
          ),
          _MetricSwitch(
            title: l10n.metricFocus,
            value: metrics.focusEnabled,
            update: (value) => metrics.copyWith(focusEnabled: value),
          ),
          _MetricSwitch(
            title: l10n.metricSleep,
            value: metrics.sleepEnabled,
            update: (value) => metrics.copyWith(sleepEnabled: value),
          ),
          const Divider(),
          ListTile(
            leading: const Icon(Icons.notifications_outlined),
            title: Text(l10n.reminderSettingsTitle),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.push('/settings/reminders'),
          ),
          ListTile(
            leading: const Icon(Icons.local_activity_outlined),
            title: Text(l10n.activityManagementTitle),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.push('/settings/activities'),
          ),
          ListTile(
            leading: const Icon(Icons.privacy_tip_outlined),
            title: Text(l10n.privacySettingsTitle),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.push('/settings/privacy'),
          ),
          const Divider(),
          ListTile(
            leading: const Icon(Icons.info_outline),
            title: Text(l10n.settingsAboutTitle),
            onTap: () => _showAbout(context, ref, l10n),
          ),
        ],
      ),
    );
  }

  Future<void> _showAbout(
    BuildContext context,
    WidgetRef ref,
    AppLocalizations l10n,
  ) async {
    final packageInfo = await ref.read(packageInfoProvider.future);
    if (!context.mounted) {
      return;
    }
    await showDialog<void>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.aboutDialogTitle),
        content: Text(
          '${l10n.aboutVersion(packageInfo.version, packageInfo.buildNumber)}\n\n${l10n.privacyBlurb}',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: Text(l10n.close),
          ),
        ],
      ),
    );
  }
}

class _MetricSwitch extends ConsumerWidget {
  const _MetricSwitch({
    required this.title,
    required this.value,
    required this.update,
  });

  final String title;
  final bool value;
  final WellbeingSettings Function(bool value) update;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return SwitchListTile(
      value: value,
      title: Text(title),
      onChanged: (enabled) => ref
          .read(settingsControllerProvider.notifier)
          .updateMetrics(update(enabled)),
    );
  }
}
