import 'package:freezed_annotation/freezed_annotation.dart';

part 'activity.freezed.dart';
part 'activity.g.dart';

/// An activity tag that can be attached to check-ins.
@freezed
abstract class Activity with _$Activity {
  /// Creates an activity domain model.
  const factory Activity({
    required String id,
    required String name,
    required bool isDefault,
    required bool isActive,
    required int sortOrder,
  }) = _Activity;

  /// Creates an activity from JSON.
  factory Activity.fromJson(Map<String, Object?> json) =>
      _$ActivityFromJson(json);
}
