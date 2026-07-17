// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'wellbeing_settings.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$WellbeingSettings {

 bool get energyEnabled; bool get anxietyEnabled; bool get focusEnabled; bool get sleepEnabled;
/// Create a copy of WellbeingSettings
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$WellbeingSettingsCopyWith<WellbeingSettings> get copyWith => _$WellbeingSettingsCopyWithImpl<WellbeingSettings>(this as WellbeingSettings, _$identity);

  /// Serializes this WellbeingSettings to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is WellbeingSettings&&(identical(other.energyEnabled, energyEnabled) || other.energyEnabled == energyEnabled)&&(identical(other.anxietyEnabled, anxietyEnabled) || other.anxietyEnabled == anxietyEnabled)&&(identical(other.focusEnabled, focusEnabled) || other.focusEnabled == focusEnabled)&&(identical(other.sleepEnabled, sleepEnabled) || other.sleepEnabled == sleepEnabled));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode => Object.hash(runtimeType,energyEnabled,anxietyEnabled,focusEnabled,sleepEnabled);

@override
String toString() {
  return 'WellbeingSettings(energyEnabled: $energyEnabled, anxietyEnabled: $anxietyEnabled, focusEnabled: $focusEnabled, sleepEnabled: $sleepEnabled)';
}


}

/// @nodoc
abstract mixin class $WellbeingSettingsCopyWith<$Res>  {
  factory $WellbeingSettingsCopyWith(WellbeingSettings value, $Res Function(WellbeingSettings) _then) = _$WellbeingSettingsCopyWithImpl;
@useResult
$Res call({
 bool energyEnabled, bool anxietyEnabled, bool focusEnabled, bool sleepEnabled
});




}
/// @nodoc
class _$WellbeingSettingsCopyWithImpl<$Res>
    implements $WellbeingSettingsCopyWith<$Res> {
  _$WellbeingSettingsCopyWithImpl(this._self, this._then);

  final WellbeingSettings _self;
  final $Res Function(WellbeingSettings) _then;

/// Create a copy of WellbeingSettings
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? energyEnabled = null,Object? anxietyEnabled = null,Object? focusEnabled = null,Object? sleepEnabled = null,}) {
  return _then(_self.copyWith(
energyEnabled: null == energyEnabled ? _self.energyEnabled : energyEnabled // ignore: cast_nullable_to_non_nullable
as bool,anxietyEnabled: null == anxietyEnabled ? _self.anxietyEnabled : anxietyEnabled // ignore: cast_nullable_to_non_nullable
as bool,focusEnabled: null == focusEnabled ? _self.focusEnabled : focusEnabled // ignore: cast_nullable_to_non_nullable
as bool,sleepEnabled: null == sleepEnabled ? _self.sleepEnabled : sleepEnabled // ignore: cast_nullable_to_non_nullable
as bool,
  ));
}

}


/// Adds pattern-matching-related methods to [WellbeingSettings].
extension WellbeingSettingsPatterns on WellbeingSettings {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _WellbeingSettings value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _WellbeingSettings() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _WellbeingSettings value)  $default,){
final _that = this;
switch (_that) {
case _WellbeingSettings():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _WellbeingSettings value)?  $default,){
final _that = this;
switch (_that) {
case _WellbeingSettings() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( bool energyEnabled,  bool anxietyEnabled,  bool focusEnabled,  bool sleepEnabled)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _WellbeingSettings() when $default != null:
return $default(_that.energyEnabled,_that.anxietyEnabled,_that.focusEnabled,_that.sleepEnabled);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( bool energyEnabled,  bool anxietyEnabled,  bool focusEnabled,  bool sleepEnabled)  $default,) {final _that = this;
switch (_that) {
case _WellbeingSettings():
return $default(_that.energyEnabled,_that.anxietyEnabled,_that.focusEnabled,_that.sleepEnabled);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( bool energyEnabled,  bool anxietyEnabled,  bool focusEnabled,  bool sleepEnabled)?  $default,) {final _that = this;
switch (_that) {
case _WellbeingSettings() when $default != null:
return $default(_that.energyEnabled,_that.anxietyEnabled,_that.focusEnabled,_that.sleepEnabled);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _WellbeingSettings implements WellbeingSettings {
  const _WellbeingSettings({this.energyEnabled = true, this.anxietyEnabled = true, this.focusEnabled = true, this.sleepEnabled = true});
  factory _WellbeingSettings.fromJson(Map<String, dynamic> json) => _$WellbeingSettingsFromJson(json);

@override@JsonKey() final  bool energyEnabled;
@override@JsonKey() final  bool anxietyEnabled;
@override@JsonKey() final  bool focusEnabled;
@override@JsonKey() final  bool sleepEnabled;

/// Create a copy of WellbeingSettings
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$WellbeingSettingsCopyWith<_WellbeingSettings> get copyWith => __$WellbeingSettingsCopyWithImpl<_WellbeingSettings>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$WellbeingSettingsToJson(this, );
}

@override
bool operator ==(Object other) {
  return identical(this, other) || (other.runtimeType == runtimeType&&other is _WellbeingSettings&&(identical(other.energyEnabled, energyEnabled) || other.energyEnabled == energyEnabled)&&(identical(other.anxietyEnabled, anxietyEnabled) || other.anxietyEnabled == anxietyEnabled)&&(identical(other.focusEnabled, focusEnabled) || other.focusEnabled == focusEnabled)&&(identical(other.sleepEnabled, sleepEnabled) || other.sleepEnabled == sleepEnabled));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode => Object.hash(runtimeType,energyEnabled,anxietyEnabled,focusEnabled,sleepEnabled);

@override
String toString() {
  return 'WellbeingSettings(energyEnabled: $energyEnabled, anxietyEnabled: $anxietyEnabled, focusEnabled: $focusEnabled, sleepEnabled: $sleepEnabled)';
}


}

/// @nodoc
abstract mixin class _$WellbeingSettingsCopyWith<$Res> implements $WellbeingSettingsCopyWith<$Res> {
  factory _$WellbeingSettingsCopyWith(_WellbeingSettings value, $Res Function(_WellbeingSettings) _then) = __$WellbeingSettingsCopyWithImpl;
@override @useResult
$Res call({
 bool energyEnabled, bool anxietyEnabled, bool focusEnabled, bool sleepEnabled
});




}
/// @nodoc
class __$WellbeingSettingsCopyWithImpl<$Res>
    implements _$WellbeingSettingsCopyWith<$Res> {
  __$WellbeingSettingsCopyWithImpl(this._self, this._then);

  final _WellbeingSettings _self;
  final $Res Function(_WellbeingSettings) _then;

/// Create a copy of WellbeingSettings
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? energyEnabled = null,Object? anxietyEnabled = null,Object? focusEnabled = null,Object? sleepEnabled = null,}) {
  return _then(_WellbeingSettings(
energyEnabled: null == energyEnabled ? _self.energyEnabled : energyEnabled // ignore: cast_nullable_to_non_nullable
as bool,anxietyEnabled: null == anxietyEnabled ? _self.anxietyEnabled : anxietyEnabled // ignore: cast_nullable_to_non_nullable
as bool,focusEnabled: null == focusEnabled ? _self.focusEnabled : focusEnabled // ignore: cast_nullable_to_non_nullable
as bool,sleepEnabled: null == sleepEnabled ? _self.sleepEnabled : sleepEnabled // ignore: cast_nullable_to_non_nullable
as bool,
  ));
}


}

// dart format on
