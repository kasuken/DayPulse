// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'app_settings.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_AppSettings _$AppSettingsFromJson(Map<String, dynamic> json) => _AppSettings(
  onboardingCompleted: json['onboardingCompleted'] as bool? ?? false,
  themeMode:
      $enumDecodeNullable(_$AppThemeModeEnumMap, json['themeMode']) ??
      AppThemeMode.system,
  reminder: json['reminder'] == null
      ? const ReminderSettings()
      : ReminderSettings.fromJson(json['reminder'] as Map<String, dynamic>),
  metrics: json['metrics'] == null
      ? const WellbeingSettings()
      : WellbeingSettings.fromJson(json['metrics'] as Map<String, dynamic>),
);

Map<String, dynamic> _$AppSettingsToJson(_AppSettings instance) =>
    <String, dynamic>{
      'onboardingCompleted': instance.onboardingCompleted,
      'themeMode': _$AppThemeModeEnumMap[instance.themeMode]!,
      'reminder': instance.reminder,
      'metrics': instance.metrics,
    };

const _$AppThemeModeEnumMap = {
  AppThemeMode.system: 'system',
  AppThemeMode.light: 'light',
  AppThemeMode.dark: 'dark',
};
