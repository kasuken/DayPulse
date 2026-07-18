import 'package:flutter/material.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:timezone/data/latest.dart' as tz_data;
import 'package:timezone/timezone.dart' as tz;

import '../errors/app_exception.dart';
import 'reminder_service.dart';

/// Provides the platform [ReminderService] implementation.
final reminderServiceProvider = Provider<ReminderService>((ref) {
  return LocalNotificationsReminderService(FlutterLocalNotificationsPlugin());
});

/// [ReminderService] backed by `flutter_local_notifications`.
class LocalNotificationsReminderService implements ReminderService {
  LocalNotificationsReminderService(this._plugin);

  static const int _dailyReminderId = 1001;
  static const String _channelId = 'daily_reminder';

  final FlutterLocalNotificationsPlugin _plugin;
  bool _initialized = false;
  bool _permissionDenied = false;

  Future<void> _ensureInitialized() async {
    if (_initialized) {
      return;
    }
    tz_data.initializeTimeZones();
    const settings = InitializationSettings(
      android: AndroidInitializationSettings('@mipmap/ic_launcher'),
      iOS: DarwinInitializationSettings(
        requestAlertPermission: false,
        requestBadgePermission: false,
        requestSoundPermission: false,
      ),
    );
    await _plugin.initialize(settings: settings);
    _initialized = true;
  }

  @override
  Future<bool> requestPermission() async {
    await _ensureInitialized();
    if (_permissionDenied) {
      return areNotificationsEnabled();
    }
    bool? granted;
    final android = _plugin
        .resolvePlatformSpecificImplementation<
          AndroidFlutterLocalNotificationsPlugin
        >();
    if (android != null) {
      granted = await android.requestNotificationsPermission();
    }
    final ios = _plugin
        .resolvePlatformSpecificImplementation<
          IOSFlutterLocalNotificationsPlugin
        >();
    if (ios != null) {
      granted = await ios.requestPermissions(alert: true, sound: true);
    }
    final result = granted ?? false;
    if (!result) {
      _permissionDenied = true;
    }
    return result;
  }

  @override
  Future<void> scheduleDailyReminder(
    TimeOfDay time, {
    required String title,
    required String body,
  }) async {
    await _ensureInitialized();
    try {
      await _plugin.cancel(id: _dailyReminderId);
      final now = tz.TZDateTime.now(tz.local);
      var scheduled = tz.TZDateTime(
        tz.local,
        now.year,
        now.month,
        now.day,
        time.hour,
        time.minute,
      );
      if (!scheduled.isAfter(now)) {
        scheduled = scheduled.add(const Duration(days: 1));
      }
      await _plugin.zonedSchedule(
        id: _dailyReminderId,
        title: title,
        body: body,
        scheduledDate: scheduled,
        notificationDetails: const NotificationDetails(
          android: AndroidNotificationDetails(
            _channelId,
            'Daily reminder',
            channelDescription: 'Reminds you to complete your daily check-in.',
            importance: Importance.defaultImportance,
            priority: Priority.defaultPriority,
          ),
          iOS: DarwinNotificationDetails(),
        ),
        androidScheduleMode: AndroidScheduleMode.inexactAllowWhileIdle,
        matchDateTimeComponents: DateTimeComponents.time,
      );
    } on Exception catch (error) {
      throw NotificationSchedulingFailure(error.runtimeType.toString());
    }
  }

  @override
  Future<void> cancelDailyReminder() async {
    await _ensureInitialized();
    await _plugin.cancel(id: _dailyReminderId);
  }

  @override
  Future<bool> areNotificationsEnabled() async {
    await _ensureInitialized();
    final android = _plugin
        .resolvePlatformSpecificImplementation<
          AndroidFlutterLocalNotificationsPlugin
        >();
    if (android != null) {
      return await android.areNotificationsEnabled() ?? false;
    }
    final ios = _plugin
        .resolvePlatformSpecificImplementation<
          IOSFlutterLocalNotificationsPlugin
        >();
    if (ios != null) {
      final options = await ios.checkPermissions();
      return options?.isEnabled ?? false;
    }
    return false;
  }
}
