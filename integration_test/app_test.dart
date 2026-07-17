import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';

import '../test/helpers/test_app.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('main wellbeing tracker flow', (tester) async {
    final TestAppHarness harness = await createTestAppHarness(
      initialPreferences: const <String, Object>{},
    );
    addTearDown(harness.close);

    await tester.pumpWidget(buildRealTestApp(harness: harness));
    await tester.pumpAndSettle();

    for (var i = 0; i < 4; i += 1) {
      await tester.tap(find.text('Continue'));
      await tester.pumpAndSettle();
    }
    await tester.tap(find.text('Start tracking'));
    await tester.pumpAndSettle();

    await tester.tap(find.text('Start check-in'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Save check-in'));
    await tester.pumpAndSettle();

    expect(find.text('Edit check-in'), findsOneWidget);

    await tester.tap(find.text('History'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('😐 Mood 0'));
    await tester.pumpAndSettle();

    await tester.tap(find.byTooltip('Edit check-in'));
    await tester.pumpAndSettle();
    await tester.enterText(find.byType(TextField), 'Updated from integration');
    await tester.tap(find.text('Save check-in'));
    await tester.pumpAndSettle();

    await tester.pageBack();
    await tester.pumpAndSettle();
    await tester.tap(find.text('😐 Mood 0'));
    await tester.pumpAndSettle();
    expect(find.text('Updated from integration'), findsOneWidget);

    await tester.pageBack();
    await tester.pumpAndSettle();
    await tester.tap(find.text('Insights'));
    await tester.pumpAndSettle();
    expect(find.text('Entries this month'), findsOneWidget);
    expect(find.text('1 entry'), findsOneWidget);
    expect(find.text('Current streak'), findsOneWidget);

    await tester.tap(find.text('History'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('😐 Mood 0'));
    await tester.pumpAndSettle();
    await tester.tap(find.byTooltip('Delete'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Delete').last);
    await tester.pumpAndSettle();

    expect(find.text('Your check-ins will appear here.'), findsOneWidget);
  });
}
