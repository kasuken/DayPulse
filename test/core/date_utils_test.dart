import 'package:daypulse/core/utilities/date_utils.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('DateUtilsX', () {
    test('dateOnly strips time using local calendar fields', () {
      final value = DateTime(2024, 5, 10, 23, 59, 1);

      expect(DateUtilsX.dateOnly(value), DateTime(2024, 5, 10));
    });

    test('startOfDay is inclusive and endOfDay is exclusive next day', () {
      final value = DateTime(2024, 5, 10, 12);

      expect(DateUtilsX.startOfDay(value), DateTime(2024, 5, 10));
      expect(DateUtilsX.endOfDay(value), DateTime(2024, 5, 11));
    });

    test('isSameDate and isNextDay use calendar days', () {
      expect(
        DateUtilsX.isSameDate(
          DateTime(2024, 5, 10, 1),
          DateTime(2024, 5, 10, 23),
        ),
        isTrue,
      );
      expect(
        DateUtilsX.isNextDay(DateTime(2024, 5, 10, 23), DateTime(2024, 5, 11)),
        isTrue,
      );
    });

    test('dateRange is inclusive and empty when reversed', () {
      expect(
        DateUtilsX.dateRange(DateTime(2024, 5, 10), DateTime(2024, 5, 12)),
        [DateTime(2024, 5, 10), DateTime(2024, 5, 11), DateTime(2024, 5, 12)],
      );
      expect(
        DateUtilsX.dateRange(DateTime(2024, 5, 12), DateTime(2024, 5, 10)),
        isEmpty,
      );
    });

    test('daysBetween compares date-only values', () {
      expect(
        DateUtilsX.daysBetween(
          DateTime(2024, 5, 10, 23),
          DateTime(2024, 5, 12, 1),
        ),
        2,
      );
    });
  });
}
