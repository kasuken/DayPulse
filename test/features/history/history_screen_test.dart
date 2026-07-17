import 'package:daypulse/features/check_in/data/drift_check_in_repository.dart';
import 'package:daypulse/features/check_in/domain/check_in.dart';
import 'package:daypulse/features/history/presentation/entry_detail_screen.dart';
import 'package:daypulse/features/history/presentation/history_screen.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:intl/intl.dart';

import '../../helpers/test_app.dart';

void main() {
  group('HistoryScreen', () {
    late TestAppHarness harness;

    setUp(() async {
      harness = await createTestAppHarness();
    });

    tearDown(() async {
      await harness.close();
    });

    testWidgets('shows empty state message', (tester) async {
      await tester.pumpWidget(
        buildTestApp(harness: harness, home: const HistoryScreen()),
      );
      await tester.pumpAndSettle();

      expect(find.text('Your check-ins will appear here.'), findsOneWidget);
    });

    testWidgets('renders entry date and mood row', (tester) async {
      final DateTime date = DateTime(2024, 1, 10);
      final DriftCheckInRepository repository = DriftCheckInRepository(
        harness.database,
      );
      await repository.create(_checkIn(id: 'one', entryDate: date, mood: 2));

      await tester.pumpWidget(
        buildTestApp(harness: harness, home: const HistoryScreen()),
      );
      await tester.pumpAndSettle();

      expect(find.text(DateFormat.yMMMMd('en').format(date)), findsOneWidget);
      expect(find.text('🙂 Mood +2'), findsOneWidget);
    });
  });

  group('EntryDetailScreen', () {
    late TestAppHarness harness;

    setUp(() async {
      harness = await createTestAppHarness();
      final DriftCheckInRepository repository = DriftCheckInRepository(
        harness.database,
      );
      await repository.create(
        _checkIn(id: 'detail', entryDate: DateTime(2024, 1, 10), mood: 1),
      );
    });

    tearDown(() async {
      await harness.close();
    });

    testWidgets('shows delete confirmation dialog text', (tester) async {
      await tester.pumpWidget(
        buildTestApp(
          harness: harness,
          home: const EntryDetailScreen(id: 'detail'),
        ),
      );
      await tester.pumpAndSettle();

      await tester.tap(find.byTooltip('Delete'));
      await tester.pumpAndSettle();

      expect(find.text('Delete this check-in?'), findsOneWidget);
      expect(find.text('This action cannot be undone.'), findsOneWidget);
    });
  });
}

CheckIn _checkIn({
  required String id,
  required DateTime entryDate,
  required int mood,
}) {
  return CheckIn(
    id: id,
    entryDate: entryDate,
    mood: mood,
    energy: 6,
    anxiety: 3,
    focus: 7,
    sleepHours: 7.5,
    activityIds: const <String>[],
    createdAt: entryDate.toUtc(),
    updatedAt: entryDate.toUtc(),
  );
}
