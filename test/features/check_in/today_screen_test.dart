import 'package:daypulse/features/check_in/data/drift_check_in_repository.dart';
import 'package:daypulse/features/check_in/domain/check_in.dart';
import 'package:daypulse/features/check_in/presentation/today_screen.dart';
import 'package:flutter_test/flutter_test.dart';

import '../../helpers/test_app.dart';

void main() {
  group('TodayScreen', () {
    late TestAppHarness harness;

    setUp(() async {
      harness = await createTestAppHarness();
    });

    tearDown(() async {
      await harness.close();
    });

    testWidgets('shows empty state prompt and start button', (tester) async {
      await tester.pumpWidget(
        buildTestApp(harness: harness, home: const TodayScreen()),
      );
      await tester.pumpAndSettle();

      expect(find.text('How are you feeling today?'), findsOneWidget);
      expect(find.text('Start check-in'), findsOneWidget);
    });

    testWidgets('shows completed state with mood and edit button', (
      tester,
    ) async {
      final DriftCheckInRepository repository = DriftCheckInRepository(
        harness.database,
      );
      await repository.create(
        _checkIn(id: 'today', entryDate: DateTime.now(), mood: 3, energy: 8),
      );

      await tester.pumpWidget(
        buildTestApp(harness: harness, home: const TodayScreen()),
      );
      await tester.pumpAndSettle();

      expect(find.text('🙂'), findsOneWidget);
      expect(find.text('Mood 3: Good'), findsOneWidget);
      expect(find.text('Edit check-in'), findsOneWidget);
    });
  });
}

CheckIn _checkIn({
  required String id,
  required DateTime entryDate,
  required int mood,
  int? energy,
}) {
  final DateTime date = DateTime(
    entryDate.year,
    entryDate.month,
    entryDate.day,
  );
  return CheckIn(
    id: id,
    entryDate: date,
    mood: mood,
    energy: energy,
    anxiety: 2,
    focus: 7,
    sleepHours: 8,
    activityIds: const <String>[],
    createdAt: date.toUtc(),
    updatedAt: date.toUtc(),
  );
}
