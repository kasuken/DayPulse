import 'package:freezed_annotation/freezed_annotation.dart';

import 'reminder_settings.dart';
import 'wellbeing_settings.dart';

part 'app_settings.freezed.dart';
part 'app_settings.g.dart';

/// Supported visual theme preferences.
enum AppThemeMode {
  /// Follow the platform theme.
  system,

  /// Always use a light theme.
  light,

  /// Always use a dark theme.
  dark,
}

/// Non-sensitive application preferences.
@freezed
abstract class AppSettings with _$AppSettings {
  /// Creates app settings.
  const factory AppSettings({
    @Default(false) bool onboardingCompleted,
    @Default(AppThemeMode.system) AppThemeMode themeMode,
    @Default(ReminderSettings()) ReminderSettings reminder,
    @Default(WellbeingSettings()) WellbeingSettings metrics,
  }) = _AppSettings;

  /// Creates app settings from JSON.
  factory AppSettings.fromJson(Map<String, Object?> json) =>
      _$AppSettingsFromJson(json);
}
