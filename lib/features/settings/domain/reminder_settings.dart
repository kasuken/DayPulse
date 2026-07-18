import 'package:freezed_annotation/freezed_annotation.dart';

part 'reminder_settings.freezed.dart';
part 'reminder_settings.g.dart';

/// Local reminder configuration.
@freezed
abstract class ReminderSettings with _$ReminderSettings {
  /// Creates reminder settings.
  const factory ReminderSettings({
    @Default(false) bool enabled,
    @Default(20) int hour,
    @Default(0) int minute,
  }) = _ReminderSettings;

  /// Creates reminder settings from JSON.
  factory ReminderSettings.fromJson(Map<String, Object?> json) =>
      _$ReminderSettingsFromJson(json);
}
