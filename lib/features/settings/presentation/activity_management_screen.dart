import 'package:flutter/material.dart';

import '../../../l10n/app_localizations.dart';

/// Placeholder for activity management.
class ActivityManagementScreen extends StatelessWidget {
  /// Creates the activity management placeholder.
  const ActivityManagementScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.activityManagementTitle)),
      body: Center(child: Text(l10n.comingSoon)),
    );
  }
}
