import 'package:flutter/material.dart';

import '../../../l10n/app_localizations.dart';

/// Placeholder for the insights tab.
class InsightsScreen extends StatelessWidget {
  /// Creates the insights placeholder.
  const InsightsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.insightsTabLabel)),
      body: Center(child: Text(l10n.comingSoon)),
    );
  }
}
