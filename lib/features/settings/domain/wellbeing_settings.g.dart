// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'wellbeing_settings.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_WellbeingSettings _$WellbeingSettingsFromJson(Map<String, dynamic> json) =>
    _WellbeingSettings(
      energyEnabled: json['energyEnabled'] as bool? ?? true,
      anxietyEnabled: json['anxietyEnabled'] as bool? ?? true,
      focusEnabled: json['focusEnabled'] as bool? ?? true,
      sleepEnabled: json['sleepEnabled'] as bool? ?? true,
    );

Map<String, dynamic> _$WellbeingSettingsToJson(_WellbeingSettings instance) =>
    <String, dynamic>{
      'energyEnabled': instance.energyEnabled,
      'anxietyEnabled': instance.anxietyEnabled,
      'focusEnabled': instance.focusEnabled,
      'sleepEnabled': instance.sleepEnabled,
    };
