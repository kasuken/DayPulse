import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/shared_prefs_settings_repository.dart';
import '../domain/app_settings.dart';
import '../domain/reminder_settings.dart';
import '../domain/wellbeing_settings.dart';

/// Exposes app settings as an [AsyncValue] and persists settings changes.
final settingsControllerProvider =
    AsyncNotifierProvider<SettingsController, AppSettings>(
      SettingsController.new,
    );

/// Application-layer controller for non-sensitive settings.
class SettingsController extends AsyncNotifier<AppSettings> {
  @override
  Future<AppSettings> build() async =>
      ref.watch(settingsRepositoryProvider).load();

  /// Updates the theme preference.
  Future<void> updateThemeMode(AppThemeMode themeMode) async {
    final previous = state.value;
    if (previous != null) {
      state = AsyncData(previous.copyWith(themeMode: themeMode));
    }
    await ref.read(settingsRepositoryProvider).updateThemeMode(themeMode);
    state = AsyncData(await ref.read(settingsRepositoryProvider).load());
  }

  /// Updates the reminder preference.
  Future<void> updateReminder(ReminderSettings reminder) async {
    final previous = state.value;
    if (previous != null) {
      state = AsyncData(previous.copyWith(reminder: reminder));
    }
    await ref.read(settingsRepositoryProvider).updateReminder(reminder);
    state = AsyncData(await ref.read(settingsRepositoryProvider).load());
  }

  /// Updates enabled optional metrics.
  Future<void> updateMetrics(WellbeingSettings metrics) async {
    final previous = state.value;
    if (previous != null) {
      state = AsyncData(previous.copyWith(metrics: metrics));
    }
    await ref.read(settingsRepositoryProvider).updateMetrics(metrics);
    state = AsyncData(await ref.read(settingsRepositoryProvider).load());
  }

  /// Marks onboarding as completed.
  Future<void> completeOnboarding() async {
    final previous = state.value;
    if (previous != null) {
      state = AsyncData(previous.copyWith(onboardingCompleted: true));
    }
    await ref.read(settingsRepositoryProvider).completeOnboarding();
    state = AsyncData(await ref.read(settingsRepositoryProvider).load());
  }
}
