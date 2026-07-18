import 'app_settings.dart';
import 'reminder_settings.dart';
import 'wellbeing_settings.dart';

/// Persistence contract for non-sensitive settings.
abstract interface class SettingsRepository {
  /// Loads the current settings.
  Future<AppSettings> load();

  /// Watches settings, emitting the current value first.
  Stream<AppSettings> watch();

  /// Updates the theme preference.
  Future<void> updateThemeMode(AppThemeMode themeMode);

  /// Updates reminder settings.
  Future<void> updateReminder(ReminderSettings reminder);

  /// Updates optional metric settings.
  Future<void> updateMetrics(WellbeingSettings metrics);

  /// Marks onboarding as complete.
  Future<void> completeOnboarding();

  /// Resets all settings to defaults.
  Future<void> reset();
}
