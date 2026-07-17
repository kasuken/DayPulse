import 'package:flutter/material.dart';

import '../../../l10n/app_localizations.dart';

/// Placeholder for appearance settings.
class AppearanceSettingsScreen extends StatelessWidget {
  /// Creates the appearance settings placeholder.
  const AppearanceSettingsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.appearanceSettingsTitle)),
      body: Center(child: Text(l10n.comingSoon)),
    );
  }
}
