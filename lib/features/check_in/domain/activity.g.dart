// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'activity.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_Activity _$ActivityFromJson(Map<String, dynamic> json) => _Activity(
  id: json['id'] as String,
  name: json['name'] as String,
  isDefault: json['isDefault'] as bool,
  isActive: json['isActive'] as bool,
  sortOrder: (json['sortOrder'] as num).toInt(),
);

Map<String, dynamic> _$ActivityToJson(_Activity instance) => <String, dynamic>{
  'id': instance.id,
  'name': instance.name,
  'isDefault': instance.isDefault,
  'isActive': instance.isActive,
  'sortOrder': instance.sortOrder,
};
