import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/notifications/local_notifications_reminder_service.dart';
import '../../settings/application/settings_controller.dart';
import '../../settings/domain/reminder_settings.dart';
import '../../settings/domain/wellbeing_settings.dart';

/// Stores in-progress onboarding choices before they are persisted.
final onboardingControllerProvider =
    NotifierProvider.autoDispose<OnboardingController, OnboardingState>(
      OnboardingController.new,
    );

/// Immutable state for the onboarding flow.
class OnboardingState {
  /// Creates onboarding state.
  const OnboardingState({
    this.metrics = const WellbeingSettings(),
    this.reminderEnabled = false,
    this.reminderTime = const TimeOfDay(hour: 20, minute: 0),
    this.permissionDenied = false,
    this.saving = false,
  });

  /// Selected optional metrics.
  final WellbeingSettings metrics;

  /// Whether a daily reminder should be enabled.
  final bool reminderEnabled;

  /// The selected daily reminder time.
  final TimeOfDay reminderTime;

  /// Whether notification permission was denied during onboarding.
  final bool permissionDenied;

  /// Whether final onboarding persistence is in progress.
  final bool saving;

  /// Returns a modified copy of this state.
  OnboardingState copyWith({
    WellbeingSettings? metrics,
    bool? reminderEnabled,
    TimeOfDay? reminderTime,
    bool? permissionDenied,
    bool? saving,
  }) => OnboardingState(
    metrics: metrics ?? this.metrics,
    reminderEnabled: reminderEnabled ?? this.reminderEnabled,
    reminderTime: reminderTime ?? this.reminderTime,
    permissionDenied: permissionDenied ?? this.permissionDenied,
    saving: saving ?? this.saving,
  );
}

/// Coordinates permission requests and persistence for onboarding.
class OnboardingController extends Notifier<OnboardingState> {
  @override
  OnboardingState build() => const OnboardingState();

  /// Updates optional metrics.
  void updateMetrics(WellbeingSettings metrics) {
    state = state.copyWith(metrics: metrics);
  }

  /// Enables or disables the daily reminder, requesting permission when needed.
  Future<void> setReminderEnabled(bool enabled) async {
    if (!enabled) {
      state = state.copyWith(reminderEnabled: false, permissionDenied: false);
      return;
    }
    final granted = await ref.read(reminderServiceProvider).requestPermission();
    state = state.copyWith(
      reminderEnabled: granted,
      permissionDenied: !granted,
    );
  }

  /// Updates the reminder time.
  void setReminderTime(TimeOfDay time) {
    state = state.copyWith(reminderTime: time);
  }

  /// Persists onboarding choices and schedules a reminder if enabled.
  Future<void> finish({
    required String reminderTitle,
    required String reminderBody,
  }) async {
    state = state.copyWith(saving: true);
    try {
      final settings = ref.read(settingsControllerProvider.notifier);
      await settings.updateMetrics(state.metrics);
      final reminder = ReminderSettings(
        enabled: state.reminderEnabled,
        hour: state.reminderTime.hour,
        minute: state.reminderTime.minute,
      );
      await settings.updateReminder(reminder);
      if (state.reminderEnabled) {
        await ref
            .read(reminderServiceProvider)
            .scheduleDailyReminder(
              state.reminderTime,
              title: reminderTitle,
              body: reminderBody,
            );
      }
      await settings.completeOnboarding();
    } finally {
      state = state.copyWith(saving: false);
    }
  }
}
