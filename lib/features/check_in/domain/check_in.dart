import 'package:freezed_annotation/freezed_annotation.dart';

part 'check_in.freezed.dart';
part 'check_in.g.dart';

/// A single wellbeing journal entry for one local calendar day.
@freezed
abstract class CheckIn with _$CheckIn {
  /// Creates a check-in domain model.
  const factory CheckIn({
    required String id,
    required DateTime entryDate,
    required int mood,
    int? energy,
    int? anxiety,
    int? focus,
    double? sleepHours,
    required List<String> activityIds,
    String? notes,
    required DateTime createdAt,
    required DateTime updatedAt,
  }) = _CheckIn;

  /// Creates a check-in from JSON.
  factory CheckIn.fromJson(Map<String, Object?> json) =>
      _$CheckInFromJson(json);
}
