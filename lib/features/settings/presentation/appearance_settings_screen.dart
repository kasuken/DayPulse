import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../l10n/app_localizations.dart';
import '../application/settings_controller.dart';
import '../domain/app_settings.dart';

/// Lets the user choose the app appearance mode.
class AppearanceSettingsScreen extends ConsumerWidget {
  /// Creates the appearance settings screen.
  const AppearanceSettingsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final mode =
        ref.watch(settingsControllerProvider).value?.themeMode ??
        AppThemeMode.system;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.appearanceSettingsTitle)),
      body: ListView(
        padding: const EdgeInsets.all(AppSpacing.md),
        children: [
          RadioGroup<AppThemeMode>(
            groupValue: mode,
            onChanged: (value) => _update(ref, value),
            child: Column(
              children: [
                RadioListTile<AppThemeMode>(
                  value: AppThemeMode.system,
                  title: Text(l10n.appearanceSystem),
                ),
                RadioListTile<AppThemeMode>(
                  value: AppThemeMode.light,
                  title: Text(l10n.appearanceLight),
                ),
                RadioListTile<AppThemeMode>(
                  value: AppThemeMode.dark,
                  title: Text(l10n.appearanceDark),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  void _update(WidgetRef ref, AppThemeMode? value) {
    if (value != null) {
      ref.read(settingsControllerProvider.notifier).updateThemeMode(value);
    }
  }
}
