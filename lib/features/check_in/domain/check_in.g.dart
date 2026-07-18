// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'check_in.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_CheckIn _$CheckInFromJson(Map<String, dynamic> json) => _CheckIn(
  id: json['id'] as String,
  entryDate: DateTime.parse(json['entryDate'] as String),
  mood: (json['mood'] as num).toInt(),
  energy: (json['energy'] as num?)?.toInt(),
  anxiety: (json['anxiety'] as num?)?.toInt(),
  focus: (json['focus'] as num?)?.toInt(),
  sleepHours: (json['sleepHours'] as num?)?.toDouble(),
  activityIds: (json['activityIds'] as List<dynamic>)
      .map((e) => e as String)
      .toList(),
  notes: json['notes'] as String?,
  createdAt: DateTime.parse(json['createdAt'] as String),
  updatedAt: DateTime.parse(json['updatedAt'] as String),
);

Map<String, dynamic> _$CheckInToJson(_CheckIn instance) => <String, dynamic>{
  'id': instance.id,
  'entryDate': instance.entryDate.toIso8601String(),
  'mood': instance.mood,
  'energy': instance.energy,
  'anxiety': instance.anxiety,
  'focus': instance.focus,
  'sleepHours': instance.sleepHours,
  'activityIds': instance.activityIds,
  'notes': instance.notes,
  'createdAt': instance.createdAt.toIso8601String(),
  'updatedAt': instance.updatedAt.toIso8601String(),
};
