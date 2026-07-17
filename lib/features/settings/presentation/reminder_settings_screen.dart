import 'package:flutter/material.dart';

import '../../../l10n/app_localizations.dart';

/// Placeholder for reminder settings.
class ReminderSettingsScreen extends StatelessWidget {
  /// Creates the reminder settings placeholder.
  const ReminderSettingsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.reminderSettingsTitle)),
      body: Center(child: Text(l10n.comingSoon)),
    );
  }
}
