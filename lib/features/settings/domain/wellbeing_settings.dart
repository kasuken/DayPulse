import 'package:freezed_annotation/freezed_annotation.dart';

part 'wellbeing_settings.freezed.dart';
part 'wellbeing_settings.g.dart';

/// Feature flags for optional wellbeing metrics.
@freezed
abstract class WellbeingSettings with _$WellbeingSettings {
  /// Creates wellbeing metric settings.
  const factory WellbeingSettings({
    @Default(true) bool energyEnabled,
    @Default(true) bool anxietyEnabled,
    @Default(true) bool focusEnabled,
    @Default(true) bool sleepEnabled,
  }) = _WellbeingSettings;

  /// Creates wellbeing settings from JSON.
  factory WellbeingSettings.fromJson(Map<String, Object?> json) =>
      _$WellbeingSettingsFromJson(json);
}
