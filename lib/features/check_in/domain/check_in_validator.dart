import '../../../core/errors/app_exception.dart';
import '../../../core/utilities/date_utils.dart';
import 'check_in.dart';

/// Validates check-in field values without performing I/O.
abstract final class CheckInValidator {
  /// Maximum note length in characters.
  static const maxNotesLength = 500;

  /// Returns all validation violations for [checkIn].
  static List<CheckInViolation> validate(CheckIn checkIn, {DateTime? now}) {
    final violations = <CheckInViolation>[];
    final today = DateUtilsX.dateOnly(now ?? DateTime.now());
    final entryDate = DateUtilsX.dateOnly(checkIn.entryDate);

    if (checkIn.mood < -5 || checkIn.mood > 5) {
      violations.add(CheckInViolation.moodOutOfRange);
    }
    if (!_isNullableIntInRange(checkIn.energy, min: 0, max: 10)) {
      violations.add(CheckInViolation.energyOutOfRange);
    }
    if (!_isNullableIntInRange(checkIn.anxiety, min: 0, max: 10)) {
      violations.add(CheckInViolation.anxietyOutOfRange);
    }
    if (!_isNullableIntInRange(checkIn.focus, min: 0, max: 10)) {
      violations.add(CheckInViolation.focusOutOfRange);
    }
    final sleepHours = checkIn.sleepHours;
    if (sleepHours != null && (sleepHours < 0 || sleepHours > 15)) {
      violations.add(CheckInViolation.sleepOutOfRange);
    }
    final notes = checkIn.notes;
    if (notes != null && notes.length > maxNotesLength) {
      violations.add(CheckInViolation.notesTooLong);
    }
    if (entryDate.isAfter(today)) {
      violations.add(CheckInViolation.invalidDate);
    }

    return violations;
  }

  /// Throws [InvalidCheckInFailure] if [checkIn] violates any rule.
  static void validateOrThrow(CheckIn checkIn, {DateTime? now}) {
    final violations = validate(checkIn, now: now);
    if (violations.isNotEmpty) {
      throw InvalidCheckInFailure(violations);
    }
  }

  static bool _isNullableIntInRange(
    int? value, {
    required int min,
    required int max,
  }) => value == null || (value >= min && value <= max);
}
