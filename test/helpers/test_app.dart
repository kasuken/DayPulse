import 'package:daypulse/app/app.dart';
import 'package:daypulse/core/database/app_database.dart';
import 'package:daypulse/core/database/database_provider.dart';
import 'package:daypulse/core/notifications/local_notifications_reminder_service.dart';
import 'package:daypulse/features/settings/data/shared_prefs_settings_repository.dart';
import 'package:daypulse/l10n/app_localizations.dart';
import 'package:drift/native.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../fakes/fake_reminder_service.dart';

class TestAppHarness {
  const TestAppHarness({
    required this.database,
    required this.preferences,
    required this.reminderService,
  });

  final AppDatabase database;
  final SharedPreferences preferences;
  final FakeReminderService reminderService;

  Future<void> close() => database.close();
}

Future<TestAppHarness> createTestAppHarness({
  Map<String, Object> initialPreferences = const <String, Object>{
    'settings.onboardingCompleted': true,
  },
  bool permissionGranted = true,
}) async {
  WidgetsFlutterBinding.ensureInitialized();
  SharedPreferences.setMockInitialValues(initialPreferences);
  final SharedPreferences preferences = await SharedPreferences.getInstance();
  return TestAppHarness(
    database: AppDatabase(NativeDatabase.memory()),
    preferences: preferences,
    reminderService: FakeReminderService(permissionGranted: permissionGranted),
  );
}

Widget buildTestApp({required TestAppHarness harness, required Widget home}) {
  return ProviderScope(
    overrides: [
      appDatabaseProvider.overrideWithValue(harness.database),
      sharedPreferencesProvider.overrideWithValue(harness.preferences),
      reminderServiceProvider.overrideWithValue(harness.reminderService),
    ],
    child: MaterialApp(
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      home: home,
    ),
  );
}

Widget buildTestRouterApp({
  required TestAppHarness harness,
  required GoRouter router,
}) {
  return ProviderScope(
    overrides: [
      appDatabaseProvider.overrideWithValue(harness.database),
      sharedPreferencesProvider.overrideWithValue(harness.preferences),
      reminderServiceProvider.overrideWithValue(harness.reminderService),
    ],
    child: MaterialApp.router(
      routerConfig: router,
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
    ),
  );
}

Widget buildRealTestApp({required TestAppHarness harness}) {
  return ProviderScope(
    overrides: [
      appDatabaseProvider.overrideWithValue(harness.database),
      sharedPreferencesProvider.overrideWithValue(harness.preferences),
      reminderServiceProvider.overrideWithValue(harness.reminderService),
    ],
    child: const DayPulseApp(),
  );
}
