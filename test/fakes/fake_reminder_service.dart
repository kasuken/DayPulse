import 'package:daypulse/core/notifications/reminder_service.dart';
import 'package:flutter/material.dart';

class ScheduledReminder {
  const ScheduledReminder({
    required this.time,
    required this.title,
    required this.body,
  });

  final TimeOfDay time;
  final String title;
  final String body;
}

class FakeReminderService implements ReminderService {
  FakeReminderService({
    this.permissionGranted = true,
    this.notificationsEnabled,
  });

  bool permissionGranted;
  int permissionRequestCount = 0;
  int cancelCount = 0;
  final List<ScheduledReminder> scheduledReminders = <ScheduledReminder>[];
  final bool? notificationsEnabled;

  @override
  Future<bool> requestPermission() async {
    permissionRequestCount += 1;
    return permissionGranted;
  }

  @override
  Future<void> scheduleDailyReminder(
    TimeOfDay time, {
    required String title,
    required String body,
  }) async {
    scheduledReminders.add(
      ScheduledReminder(time: time, title: title, body: body),
    );
  }

  @override
  Future<void> cancelDailyReminder() async {
    cancelCount += 1;
    scheduledReminders.clear();
  }

  @override
  Future<bool> areNotificationsEnabled() async =>
      notificationsEnabled ?? permissionGranted;
}
