import 'package:daypulse/features/check_in/data/drift_check_in_repository.dart';
import 'package:daypulse/features/check_in/presentation/check_in_screen.dart';
import 'package:daypulse/features/check_in/presentation/today_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';

import '../../helpers/test_app.dart';

void main() {
  group('CheckInScreen', () {
    late TestAppHarness harness;

    setUp(() async {
      harness = await createTestAppHarness();
    });

    tearDown(() async {
      await harness.close();
    });

    testWidgets('renders mood slider, metric sliders, and notes counter', (
      tester,
    ) async {
      await tester.pumpWidget(_buildCheckInApp(harness));
      await tester.pumpAndSettle();

      expect(find.text('Mood'), findsOneWidget);
      expect(find.text('Energy'), findsOneWidget);
      expect(find.text('Anxiety'), findsOneWidget);
      expect(find.text('Focus'), findsOneWidget);
      expect(find.text('Sleep'), findsOneWidget);
      expect(find.text('0/500'), findsOneWidget);
      expect(find.byType(Slider), findsNWidgets(5));
    });

    testWidgets('saving creates a check-in row', (tester) async {
      final DriftCheckInRepository repository = DriftCheckInRepository(
        harness.database,
      );
      await tester.pumpWidget(_buildCheckInApp(harness));
      await tester.pumpAndSettle();

      await tester.tap(find.text('Save check-in'));
      await tester.pumpAndSettle();

      final entries = await repository.getAll();
      expect(entries, hasLength(1));
      expect(entries.single.mood, 0);
    });

    testWidgets('hides disabled metrics from settings', (tester) async {
      await harness.close();
      harness = await createTestAppHarness(
        initialPreferences: const <String, Object>{
          'settings.onboardingCompleted': true,
          'settings.metrics.energyEnabled': false,
        },
      );

      await tester.pumpWidget(_buildCheckInApp(harness));
      await tester.pumpAndSettle();

      expect(find.text('Energy'), findsNothing);
      expect(find.text('Anxiety'), findsOneWidget);
      expect(find.text('Focus'), findsOneWidget);
      expect(find.text('Sleep'), findsOneWidget);
    });
  });
}

Widget _buildCheckInApp(TestAppHarness harness) {
  final GoRouter router = GoRouter(
    initialLocation: '/check-in',
    routes: <RouteBase>[
      GoRoute(
        path: '/check-in',
        builder: (context, state) => const CheckInScreen(),
      ),
      GoRoute(path: '/today', builder: (context, state) => const TodayScreen()),
    ],
  );
  return buildTestRouterApp(harness: harness, router: router);
}
