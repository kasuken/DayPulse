import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../domain/app_settings.dart';
import '../domain/reminder_settings.dart';
import '../domain/settings_repository.dart';
import '../domain/wellbeing_settings.dart';

/// SharedPreferences-backed implementation for non-sensitive settings.
final class SharedPrefsSettingsRepository implements SettingsRepository {
  /// Creates a repository using [preferences].
  SharedPrefsSettingsRepository(this._preferences);

  static const _onboardingCompletedKey = 'settings.onboardingCompleted';
  static const _themeModeKey = 'settings.themeMode';
  static const _reminderEnabledKey = 'settings.reminder.enabled';
  static const _reminderHourKey = 'settings.reminder.hour';
  static const _reminderMinuteKey = 'settings.reminder.minute';
  static const _energyEnabledKey = 'settings.metrics.energyEnabled';
  static const _anxietyEnabledKey = 'settings.metrics.anxietyEnabled';
  static const _focusEnabledKey = 'settings.metrics.focusEnabled';
  static const _sleepEnabledKey = 'settings.metrics.sleepEnabled';

  final SharedPreferences _preferences;
  final StreamController<AppSettings> _controller =
      StreamController<AppSettings>.broadcast();

  @override
  Future<AppSettings> load() async => _readSettings();

  @override
  Stream<AppSettings> watch() async* {
    yield _readSettings();
    yield* _controller.stream;
  }

  @override
  Future<void> updateThemeMode(AppThemeMode themeMode) async {
    await _preferences.setString(_themeModeKey, themeMode.name);
    _emit();
  }

  @override
  Future<void> updateReminder(ReminderSettings reminder) async {
    await _preferences.setBool(_reminderEnabledKey, reminder.enabled);
    await _preferences.setInt(_reminderHourKey, reminder.hour);
    await _preferences.setInt(_reminderMinuteKey, reminder.minute);
    _emit();
  }

  @override
  Future<void> updateMetrics(WellbeingSettings metrics) async {
    await _preferences.setBool(_energyEnabledKey, metrics.energyEnabled);
    await _preferences.setBool(_anxietyEnabledKey, metrics.anxietyEnabled);
    await _preferences.setBool(_focusEnabledKey, metrics.focusEnabled);
    await _preferences.setBool(_sleepEnabledKey, metrics.sleepEnabled);
    _emit();
  }

  @override
  Future<void> completeOnboarding() async {
    await _preferences.setBool(_onboardingCompletedKey, true);
    _emit();
  }

  @override
  Future<void> reset() async {
    for (final key in _allKeys) {
      await _preferences.remove(key);
    }
    _emit();
  }

  /// Closes the settings stream controller.
  Future<void> dispose() => _controller.close();

  AppSettings _readSettings() => AppSettings(
    onboardingCompleted: _preferences.getBool(_onboardingCompletedKey) ?? false,
    themeMode: _readThemeMode(),
    reminder: ReminderSettings(
      enabled: _preferences.getBool(_reminderEnabledKey) ?? false,
      hour: _preferences.getInt(_reminderHourKey) ?? 20,
      minute: _preferences.getInt(_reminderMinuteKey) ?? 0,
    ),
    metrics: WellbeingSettings(
      energyEnabled: _preferences.getBool(_energyEnabledKey) ?? true,
      anxietyEnabled: _preferences.getBool(_anxietyEnabledKey) ?? true,
      focusEnabled: _preferences.getBool(_focusEnabledKey) ?? true,
      sleepEnabled: _preferences.getBool(_sleepEnabledKey) ?? true,
    ),
  );

  AppThemeMode _readThemeMode() {
    final stored = _preferences.getString(_themeModeKey);
    return AppThemeMode.values.firstWhere(
      (mode) => mode.name == stored,
      orElse: () => AppThemeMode.system,
    );
  }

  void _emit() {
    if (!_controller.isClosed) {
      _controller.add(_readSettings());
    }
  }

  static const _allKeys = <String>[
    _onboardingCompletedKey,
    _themeModeKey,
    _reminderEnabledKey,
    _reminderHourKey,
    _reminderMinuteKey,
    _energyEnabledKey,
    _anxietyEnabledKey,
    _focusEnabledKey,
    _sleepEnabledKey,
  ];
}

/// Provides SharedPreferences; override during app bootstrap after initialization.
final sharedPreferencesProvider = Provider<SharedPreferences>((ref) {
  throw StateError('Override sharedPreferencesProvider during app bootstrap.');
});

/// Provides the settings repository.
final settingsRepositoryProvider = Provider<SettingsRepository>((ref) {
  final preferences = ref.watch(sharedPreferencesProvider);
  final repository = SharedPrefsSettingsRepository(preferences);
  ref.onDispose(repository.dispose);
  return repository;
});
