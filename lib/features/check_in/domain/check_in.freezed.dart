// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'check_in.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$CheckIn {

 String get id; DateTime get entryDate; int get mood; int? get energy; int? get anxiety; int? get focus; double? get sleepHours; List<String> get activityIds; String? get notes; DateTime get createdAt; DateTime get updatedAt;
/// Create a copy of CheckIn
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$CheckInCopyWith<CheckIn> get copyWith => _$CheckInCopyWithImpl<CheckIn>(this as CheckIn, _$identity);

  /// Serializes this CheckIn to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is CheckIn&&(identical(other.id, id) || other.id == id)&&(identical(other.entryDate, entryDate) || other.entryDate == entryDate)&&(identical(other.mood, mood) || other.mood == mood)&&(identical(other.energy, energy) || other.energy == energy)&&(identical(other.anxiety, anxiety) || other.anxiety == anxiety)&&(identical(other.focus, focus) || other.focus == focus)&&(identical(other.sleepHours, sleepHours) || other.sleepHours == sleepHours)&&const DeepCollectionEquality().equals(other.activityIds, activityIds)&&(identical(other.notes, notes) || other.notes == notes)&&(identical(other.createdAt, createdAt) || other.createdAt == createdAt)&&(identical(other.updatedAt, updatedAt) || other.updatedAt == updatedAt));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode => Object.hash(runtimeType,id,entryDate,mood,energy,anxiety,focus,sleepHours,const DeepCollectionEquality().hash(activityIds),notes,createdAt,updatedAt);

@override
String toString() {
  return 'CheckIn(id: $id, entryDate: $entryDate, mood: $mood, energy: $energy, anxiety: $anxiety, focus: $focus, sleepHours: $sleepHours, activityIds: $activityIds, notes: $notes, createdAt: $createdAt, updatedAt: $updatedAt)';
}


}

/// @nodoc
abstract mixin class $CheckInCopyWith<$Res>  {
  factory $CheckInCopyWith(CheckIn value, $Res Function(CheckIn) _then) = _$CheckInCopyWithImpl;
@useResult
$Res call({
 String id, DateTime entryDate, int mood, int? energy, int? anxiety, int? focus, double? sleepHours, List<String> activityIds, String? notes, DateTime createdAt, DateTime updatedAt
});




}
/// @nodoc
class _$CheckInCopyWithImpl<$Res>
    implements $CheckInCopyWith<$Res> {
  _$CheckInCopyWithImpl(this._self, this._then);

  final CheckIn _self;
  final $Res Function(CheckIn) _then;

/// Create a copy of CheckIn
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? id = null,Object? entryDate = null,Object? mood = null,Object? energy = freezed,Object? anxiety = freezed,Object? focus = freezed,Object? sleepHours = freezed,Object? activityIds = null,Object? notes = freezed,Object? createdAt = null,Object? updatedAt = null,}) {
  return _then(_self.copyWith(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,entryDate: null == entryDate ? _self.entryDate : entryDate // ignore: cast_nullable_to_non_nullable
as DateTime,mood: null == mood ? _self.mood : mood // ignore: cast_nullable_to_non_nullable
as int,energy: freezed == energy ? _self.energy : energy // ignore: cast_nullable_to_non_nullable
as int?,anxiety: freezed == anxiety ? _self.anxiety : anxiety // ignore: cast_nullable_to_non_nullable
as int?,focus: freezed == focus ? _self.focus : focus // ignore: cast_nullable_to_non_nullable
as int?,sleepHours: freezed == sleepHours ? _self.sleepHours : sleepHours // ignore: cast_nullable_to_non_nullable
as double?,activityIds: null == activityIds ? _self.activityIds : activityIds // ignore: cast_nullable_to_non_nullable
as List<String>,notes: freezed == notes ? _self.notes : notes // ignore: cast_nullable_to_non_nullable
as String?,createdAt: null == createdAt ? _self.createdAt : createdAt // ignore: cast_nullable_to_non_nullable
as DateTime,updatedAt: null == updatedAt ? _self.updatedAt : updatedAt // ignore: cast_nullable_to_non_nullable
as DateTime,
  ));
}

}


/// Adds pattern-matching-related methods to [CheckIn].
extension CheckInPatterns on CheckIn {
/// A variant of `map` that fallback to returning `orElse`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _CheckIn value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _CheckIn() when $default != null:
return $default(_that);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// Callbacks receives the raw object, upcasted.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case final Subclass2 value:
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _CheckIn value)  $default,){
final _that = this;
switch (_that) {
case _CheckIn():
return $default(_that);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `map` that fallback to returning `null`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _CheckIn value)?  $default,){
final _that = this;
switch (_that) {
case _CheckIn() when $default != null:
return $default(_that);case _:
  return null;

}
}
/// A variant of `when` that fallback to an `orElse` callback.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String id,  DateTime entryDate,  int mood,  int? energy,  int? anxiety,  int? focus,  double? sleepHours,  List<String> activityIds,  String? notes,  DateTime createdAt,  DateTime updatedAt)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _CheckIn() when $default != null:
return $default(_that.id,_that.entryDate,_that.mood,_that.energy,_that.anxiety,_that.focus,_that.sleepHours,_that.activityIds,_that.notes,_that.createdAt,_that.updatedAt);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// As opposed to `map`, this offers destructuring.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case Subclass2(:final field2):
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String id,  DateTime entryDate,  int mood,  int? energy,  int? anxiety,  int? focus,  double? sleepHours,  List<String> activityIds,  String? notes,  DateTime createdAt,  DateTime updatedAt)  $default,) {final _that = this;
switch (_that) {
case _CheckIn():
return $default(_that.id,_that.entryDate,_that.mood,_that.energy,_that.anxiety,_that.focus,_that.sleepHours,_that.activityIds,_that.notes,_that.createdAt,_that.updatedAt);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `when` that fallback to returning `null`
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String id,  DateTime entryDate,  int mood,  int? energy,  int? anxiety,  int? focus,  double? sleepHours,  List<String> activityIds,  String? notes,  DateTime createdAt,  DateTime updatedAt)?  $default,) {final _that = this;
switch (_that) {
case _CheckIn() when $default != null:
return $default(_that.id,_that.entryDate,_that.mood,_that.energy,_that.anxiety,_that.focus,_that.sleepHours,_that.activityIds,_that.notes,_that.createdAt,_that.updatedAt);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _CheckIn implements CheckIn {
  const _CheckIn({required this.id, required this.entryDate, required this.mood, this.energy, this.anxiety, this.focus, this.sleepHours, required final  List<String> activityIds, this.notes, required this.createdAt, required this.updatedAt}): _activityIds = activityIds;
  factory _CheckIn.fromJson(Map<String, dynamic> json) => _$CheckInFromJson(json);

@override final  String id;
@override final  DateTime entryDate;
@override final  int mood;
@override final  int? energy;
@override final  int? anxiety;
@override final  int? focus;
@override final  double? sleepHours;
 final  List<String> _activityIds;
@override List<String> get activityIds {
  if (_activityIds is EqualUnmodifiableListView) return _activityIds;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_activityIds);
}

@override final  String? notes;
@override final  DateTime createdAt;
@override final  DateTime updatedAt;

/// Create a copy of CheckIn
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$CheckInCopyWith<_CheckIn> get copyWith => __$CheckInCopyWithImpl<_CheckIn>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$CheckInToJson(this, );
}

@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is _CheckIn&&(identical(other.id, id) || other.id == id)&&(identical(other.entryDate, entryDate) || other.entryDate == entryDate)&&(identical(other.mood, mood) || other.mood == mood)&&(identical(other.energy, energy) || other.energy == energy)&&(identical(other.anxiety, anxiety) || other.anxiety == anxiety)&&(identical(other.focus, focus) || other.focus == focus)&&(identical(other.sleepHours, sleepHours) || other.sleepHours == sleepHours)&&const DeepCollectionEquality().equals(other._activityIds, _activityIds)&&(identical(other.notes, notes) || other.notes == notes)&&(identical(other.createdAt, createdAt) || other.createdAt == createdAt)&&(identical(other.updatedAt, updatedAt) || other.updatedAt == updatedAt));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode => Object.hash(runtimeType,id,entryDate,mood,energy,anxiety,focus,sleepHours,const DeepCollectionEquality().hash(_activityIds),notes,createdAt,updatedAt);

@override
String toString() {
  return 'CheckIn(id: $id, entryDate: $entryDate, mood: $mood, energy: $energy, anxiety: $anxiety, focus: $focus, sleepHours: $sleepHours, activityIds: $activityIds, notes: $notes, createdAt: $createdAt, updatedAt: $updatedAt)';
}


}

/// @nodoc
abstract mixin class _$CheckInCopyWith<$Res> implements $CheckInCopyWith<$Res> {
  factory _$CheckInCopyWith(_CheckIn value, $Res Function(_CheckIn) _then) = __$CheckInCopyWithImpl;
@override @useResult
$Res call({
 String id, DateTime entryDate, int mood, int? energy, int? anxiety, int? focus, double? sleepHours, List<String> activityIds, String? notes, DateTime createdAt, DateTime updatedAt
});




}
/// @nodoc
class __$CheckInCopyWithImpl<$Res>
    implements _$CheckInCopyWith<$Res> {
  __$CheckInCopyWithImpl(this._self, this._then);

  final _CheckIn _self;
  final $Res Function(_CheckIn) _then;

/// Create a copy of CheckIn
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? id = null,Object? entryDate = null,Object? mood = null,Object? energy = freezed,Object? anxiety = freezed,Object? focus = freezed,Object? sleepHours = freezed,Object? activityIds = null,Object? notes = freezed,Object? createdAt = null,Object? updatedAt = null,}) {
  return _then(_CheckIn(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,entryDate: null == entryDate ? _self.entryDate : entryDate // ignore: cast_nullable_to_non_nullable
as DateTime,mood: null == mood ? _self.mood : mood // ignore: cast_nullable_to_non_nullable
as int,energy: freezed == energy ? _self.energy : energy // ignore: cast_nullable_to_non_nullable
as int?,anxiety: freezed == anxiety ? _self.anxiety : anxiety // ignore: cast_nullable_to_non_nullable
as int?,focus: freezed == focus ? _self.focus : focus // ignore: cast_nullable_to_non_nullable
as int?,sleepHours: freezed == sleepHours ? _self.sleepHours : sleepHours // ignore: cast_nullable_to_non_nullable
as double?,activityIds: null == activityIds ? _self._activityIds : activityIds // ignore: cast_nullable_to_non_nullable
as List<String>,notes: freezed == notes ? _self.notes : notes // ignore: cast_nullable_to_non_nullable
as String?,createdAt: null == createdAt ? _self.createdAt : createdAt // ignore: cast_nullable_to_non_nullable
as DateTime,updatedAt: null == updatedAt ? _self.updatedAt : updatedAt // ignore: cast_nullable_to_non_nullable
as DateTime,
  ));
}


}

// dart format on
