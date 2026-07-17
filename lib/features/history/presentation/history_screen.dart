import 'package:flutter/material.dart';

import '../../../l10n/app_localizations.dart';

/// Placeholder for the history tab.
class HistoryScreen extends StatelessWidget {
  /// Creates the history placeholder.
  const HistoryScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.historyTabLabel)),
      body: Center(child: Text(l10n.comingSoon)),
    );
  }
}
