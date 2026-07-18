import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'app_database.dart';

/// Provides the application database and closes it when the provider is disposed.
final appDatabaseProvider = Provider<AppDatabase>((ref) {
  final database = AppDatabase();
  ref.onDispose(database.close);
  return database;
});
