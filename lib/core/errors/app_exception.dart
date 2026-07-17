/// Domain-level failures surfaced to the UI as user-friendly messages.
sealed class AppException implements Exception {
  const AppException([this.message]);

  /// Technical detail for logging; never shown to the user and never
  /// contains personal wellbeing data.
  final String? message;
}

/// A database read or write failed.
final class DatabaseFailure extends AppException {
  const DatabaseFailure([super.message]);
}

/// A check-in already exists for the requested calendar date.
final class DuplicateEntryDateFailure extends AppException {
  const DuplicateEntryDateFailure([super.message]);
}

/// Check-in values are outside the allowed ranges.
final class InvalidCheckInFailure extends AppException {
  const InvalidCheckInFailure(this.violations, [String? message])
    : super(message);

  /// Machine-readable list of violated rules.
  final List<CheckInViolation> violations;
}

/// The notification permission was denied by the user or the system.
final class NotificationPermissionFailure extends AppException {
  const NotificationPermissionFailure([super.message]);
}

/// Scheduling a local notification failed.
final class NotificationSchedulingFailure extends AppException {
  const NotificationSchedulingFailure([super.message]);
}

/// Exporting data failed.
final class ExportFailure extends AppException {
  const ExportFailure([super.message]);
}

/// Validation rules that a check-in can violate.
enum CheckInViolation {
  moodOutOfRange,
  energyOutOfRange,
  anxietyOutOfRange,
  focusOutOfRange,
  sleepOutOfRange,
  notesTooLong,
  invalidDate,
}
