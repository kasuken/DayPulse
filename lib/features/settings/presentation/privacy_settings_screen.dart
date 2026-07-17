import 'package:flutter/material.dart';

import '../../../l10n/app_localizations.dart';

/// Placeholder for privacy settings.
class PrivacySettingsScreen extends StatelessWidget {
  /// Creates the privacy settings placeholder.
  const PrivacySettingsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.privacySettingsTitle)),
      body: Center(child: Text(l10n.comingSoon)),
    );
  }
}
