import 'package:daypulse/core/errors/app_exception.dart';
import 'package:daypulse/features/check_in/domain/check_in.dart';
import 'package:daypulse/features/check_in/domain/check_in_validator.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('CheckInValidator', () {
    test('accepts boundary values', () {
      final checkIn = _checkIn(
        mood: -5,
        energy: 0,
        anxiety: 10,
        focus: 0,
        sleepHours: 15,
        notes: 'x' * 500,
      );

      expect(
        CheckInValidator.validate(checkIn, now: DateTime(2024, 1, 1)),
        isEmpty,
      );
    });

    test('returns every violated range rule', () {
      final checkIn = _checkIn(
        mood: 6,
        energy: 11,
        anxiety: -1,
        focus: 12,
        sleepHours: 15.5,
        notes: 'x' * 501,
        entryDate: DateTime(2024, 1, 2),
      );

      expect(
        CheckInValidator.validate(checkIn, now: DateTime(2024, 1, 1)),
        containsAll(<CheckInViolation>[
          CheckInViolation.moodOutOfRange,
          CheckInViolation.energyOutOfRange,
          CheckInViolation.anxietyOutOfRange,
          CheckInViolation.focusOutOfRange,
          CheckInViolation.sleepOutOfRange,
          CheckInViolation.notesTooLong,
          CheckInViolation.invalidDate,
        ]),
      );
    });

    test('validateOrThrow throws InvalidCheckInFailure', () {
      final checkIn = _checkIn(mood: -6);

      expect(
        () => CheckInValidator.validateOrThrow(
          checkIn,
          now: DateTime(2024, 1, 1),
        ),
        throwsA(isA<InvalidCheckInFailure>()),
      );
    });
  });
}

CheckIn _checkIn({
  int mood = 0,
  int? energy,
  int? anxiety,
  int? focus,
  double? sleepHours,
  String? notes,
  DateTime? entryDate,
}) {
  final now = DateTime.utc(2024);
  return CheckIn(
    id: 'id',
    entryDate: entryDate ?? DateTime(2024, 1, 1),
    mood: mood,
    energy: energy,
    anxiety: anxiety,
    focus: focus,
    sleepHours: sleepHours,
    activityIds: const [],
    notes: notes,
    createdAt: now,
    updatedAt: now,
  );
}
