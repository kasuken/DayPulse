import 'package:flutter/material.dart';

import '../../../l10n/app_localizations.dart';

/// Placeholder for settings.
class SettingsScreen extends StatelessWidget {
  /// Creates the settings placeholder.
  const SettingsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.settingsTabLabel)),
      body: Center(child: Text(l10n.comingSoon)),
    );
  }
}
