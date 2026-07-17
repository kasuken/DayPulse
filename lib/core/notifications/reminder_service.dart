import 'package:flutter/material.dart';

/// Abstraction over platform local-notification scheduling.
///
/// The application layer depends only on this interface; the
/// platform-specific implementation lives alongside it and is injected
/// through Riverpod. Tests use a fake implementation.
abstract interface class ReminderService {
  /// Requests notification permission from the operating system.
  ///
  /// Returns `true` when permission is granted. Implementations must not
  /// repeatedly prompt after a denial; they should return the last known
  /// status instead.
  Future<bool> requestPermission();

  /// Schedules one repeating daily reminder at [time] (local time zone).
  ///
  /// [title] and [body] are the localized notification texts; they must
  /// not contain mental-health information suitable only for private view.
  /// Replaces any previously scheduled reminder.
  Future<void> scheduleDailyReminder(
    TimeOfDay time, {
    required String title,
    required String body,
  });

  /// Cancels the daily reminder if one is scheduled.
  Future<void> cancelDailyReminder();

  /// Whether notifications are currently enabled at the OS level.
  Future<bool> areNotificationsEnabled();
}
