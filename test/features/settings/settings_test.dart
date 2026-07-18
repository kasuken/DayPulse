import 'package:daypulse/features/check_in/presentation/today_screen.dart';
import 'package:daypulse/features/onboarding/presentation/onboarding_screen.dart';
import 'package:daypulse/features/settings/domain/app_settings.dart';
import 'package:daypulse/features/settings/presentation/appearance_settings_screen.dart';
import 'package:daypulse/features/settings/presentation/settings_screen.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';

import '../../helpers/test_app.dart';

void main() {
  group('Settings screens', () {
    late TestAppHarness harness;

    setUp(() async {
      harness = await createTestAppHarness();
    });

    tearDown(() async {
      await harness.close();
    });

    testWidgets('appearance screen persists dark theme selection', (
      tester,
    ) async {
      await tester.pumpWidget(
        buildTestApp(harness: harness, home: const AppearanceSettingsScreen()),
      );
      await tester.pumpAndSettle();

      await tester.tap(find.text('Dark'));
      await tester.pumpAndSettle();

      expect(
        harness.preferences.getString('settings.themeMode'),
        AppThemeMode.dark.name,
      );
    });

    testWidgets('metric switch toggle persists metric settings', (
      tester,
    ) async {
      await tester.pumpWidget(
        buildTestApp(harness: harness, home: const SettingsScreen()),
      );
      await tester.pumpAndSettle();

      await tester.tap(find.text('Energy'));
      await tester.pumpAndSettle();

      expect(
        harness.preferences.getBool('settings.metrics.energyEnabled'),
        isFalse,
      );
    });
  });

  group('OnboardingScreen', () {
    late TestAppHarness harness;

    setUp(() async {
      harness = await createTestAppHarness(
        initialPreferences: const <String, Object>{},
      );
    });

    tearDown(() async {
      await harness.close();
    });

    testWidgets('completes onboarding and persists the flag', (tester) async {
      final GoRouter router = GoRouter(
        initialLocation: '/onboarding',
        routes: <RouteBase>[
          GoRoute(
            path: '/onboarding',
            builder: (context, state) => const OnboardingScreen(),
          ),
          GoRoute(
            path: '/today',
            builder: (context, state) => const TodayScreen(),
          ),
        ],
      );
      await tester.pumpWidget(
        buildTestRouterApp(harness: harness, router: router),
      );
      await tester.pumpAndSettle();

      for (var i = 0; i < 4; i += 1) {
        await tester.tap(find.text('Continue'));
        await tester.pumpAndSettle();
      }
      await tester.tap(find.text('Start tracking'));
      await tester.pumpAndSettle();

      expect(
        harness.preferences.getBool('settings.onboardingCompleted'),
        isTrue,
      );
    });
  });
}
