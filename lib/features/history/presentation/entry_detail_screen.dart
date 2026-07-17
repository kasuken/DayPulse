import 'package:flutter/material.dart';

import '../../../l10n/app_localizations.dart';

/// Placeholder for a check-in detail screen.
class EntryDetailScreen extends StatelessWidget {
  /// Creates the entry detail placeholder.
  const EntryDetailScreen({required this.id, super.key});

  /// Entry identifier.
  final String id;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.entryDetailsTitle)),
      body: Center(child: Text(l10n.comingSoon)),
    );
  }
}
