import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../features/settings/domain/wellbeing_settings.dart';
import '../../../l10n/app_localizations.dart';
import '../application/onboarding_controller.dart';

/// Five-step onboarding flow for first launch.
class OnboardingScreen extends ConsumerStatefulWidget {
  /// Creates the onboarding screen.
  const OnboardingScreen({super.key});

  @override
  ConsumerState<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends ConsumerState<OnboardingScreen> {
  static const _steps = 5;
  final _pageController = PageController();
  int _index = 0;

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final state = ref.watch(onboardingControllerProvider);
    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Column(
            children: [
              Semantics(
                label: l10n.onboardingProgress(_index + 1, _steps),
                child: LinearProgressIndicator(value: (_index + 1) / _steps),
              ),
              const SizedBox(height: AppSpacing.xl),
              Expanded(
                child: PageView(
                  controller: _pageController,
                  onPageChanged: (value) => setState(() => _index = value),
                  children: [
                    _IntroStep(
                      title: l10n.onboardingWelcomeTitle,
                      body: l10n.onboardingWelcomeTagline,
                      icon: Icons.favorite_outline,
                    ),
                    _PrivacyStep(l10n: l10n),
                    _MetricsStep(l10n: l10n, metrics: state.metrics),
                    _ReminderStep(l10n: l10n, state: state),
                    _IntroStep(
                      title: l10n.onboardingFinishTitle,
                      body: l10n.onboardingFinishDescription,
                      icon: Icons.check_circle_outline,
                    ),
                  ],
                ),
              ),
              const SizedBox(height: AppSpacing.lg),
              Row(
                children: [
                  if (_index > 0)
                    Expanded(
                      child: OutlinedButton(
                        onPressed: state.saving ? null : _previous,
                        child: Text(l10n.onboardingBack),
                      ),
                    ),
                  if (_index > 0) const SizedBox(width: AppSpacing.md),
                  Expanded(
                    child: FilledButton(
                      onPressed: state.saving
                          ? null
                          : () => _nextOrFinish(l10n),
                      child: Text(
                        _index == _steps - 1
                            ? l10n.startTracking
                            : l10n.onboardingContinue,
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _previous() {
    _pageController.previousPage(
      duration: const Duration(milliseconds: 250),
      curve: Curves.easeOut,
    );
  }

  Future<void> _nextOrFinish(AppLocalizations l10n) async {
    if (_index < _steps - 1) {
      await _pageController.nextPage(
        duration: const Duration(milliseconds: 250),
        curve: Curves.easeOut,
      );
      return;
    }
    await ref
        .read(onboardingControllerProvider.notifier)
        .finish(
          reminderTitle: l10n.dailyReminderTitle,
          reminderBody: l10n.dailyReminderBody,
        );
    if (mounted) {
      context.go('/today');
    }
  }
}

class _IntroStep extends StatelessWidget {
  const _IntroStep({
    required this.title,
    required this.body,
    required this.icon,
  });

  final String title;
  final String body;
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Icon(icon, size: 72, color: Theme.of(context).colorScheme.primary),
        const SizedBox(height: AppSpacing.xl),
        Text(
          title,
          style: textTheme.headlineMedium,
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: AppSpacing.md),
        Text(body, style: textTheme.bodyLarge, textAlign: TextAlign.center),
      ],
    );
  }
}

class _PrivacyStep extends StatelessWidget {
  const _PrivacyStep({required this.l10n});

  final AppLocalizations l10n;

  @override
  Widget build(BuildContext context) {
    final bullets = [
      l10n.onboardingPrivacyLocal,
      l10n.onboardingPrivacyNoAccount,
      l10n.onboardingPrivacyNoUpload,
      l10n.onboardingPrivacyDelete,
    ];
    return ListView(
      children: [
        Text(
          l10n.onboardingPrivacyTitle,
          style: Theme.of(context).textTheme.headlineMedium,
        ),
        const SizedBox(height: AppSpacing.lg),
        for (final bullet in bullets)
          ListTile(
            leading: const Icon(Icons.check_circle_outline),
            title: Text(bullet),
          ),
      ],
    );
  }
}

class _MetricsStep extends ConsumerWidget {
  const _MetricsStep({required this.l10n, required this.metrics});

  final AppLocalizations l10n;
  final WellbeingSettings metrics;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return ListView(
      children: [
        Text(
          l10n.onboardingMetricsTitle,
          style: Theme.of(context).textTheme.headlineMedium,
        ),
        const SizedBox(height: AppSpacing.lg),
        SwitchListTile(
          value: true,
          onChanged: null,
          title: Text(l10n.metricMood),
          subtitle: Text(l10n.onboardingMoodAlwaysOn),
        ),
        _metricSwitch(
          l10n.metricEnergy,
          metrics.energyEnabled,
          (value) => metrics.copyWith(energyEnabled: value),
          ref,
        ),
        _metricSwitch(
          l10n.metricAnxiety,
          metrics.anxietyEnabled,
          (value) => metrics.copyWith(anxietyEnabled: value),
          ref,
        ),
        _metricSwitch(
          l10n.metricFocus,
          metrics.focusEnabled,
          (value) => metrics.copyWith(focusEnabled: value),
          ref,
        ),
        _metricSwitch(
          l10n.metricSleep,
          metrics.sleepEnabled,
          (value) => metrics.copyWith(sleepEnabled: value),
          ref,
        ),
      ],
    );
  }

  Widget _metricSwitch(
    String title,
    bool value,
    WellbeingSettings Function(bool value) update,
    WidgetRef ref,
  ) => SwitchListTile(
    value: value,
    title: Text(title),
    onChanged: (enabled) => ref
        .read(onboardingControllerProvider.notifier)
        .updateMetrics(update(enabled)),
  );
}

class _ReminderStep extends ConsumerWidget {
  const _ReminderStep({required this.l10n, required this.state});

  final AppLocalizations l10n;
  final OnboardingState state;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return ListView(
      children: [
        Text(
          l10n.onboardingReminderTitle,
          style: Theme.of(context).textTheme.headlineMedium,
        ),
        const SizedBox(height: AppSpacing.sm),
        Text(l10n.onboardingReminderDescription),
        const SizedBox(height: AppSpacing.lg),
        SwitchListTile(
          value: state.reminderEnabled,
          title: Text(l10n.reminderSwitchLabel),
          onChanged: (value) => ref
              .read(onboardingControllerProvider.notifier)
              .setReminderEnabled(value),
        ),
        ListTile(
          title: Text(l10n.reminderTimeLabel),
          subtitle: Text(state.reminderTime.format(context)),
          trailing: OutlinedButton(
            onPressed: state.reminderEnabled
                ? () => _pickTime(context, ref)
                : null,
            child: Text(l10n.changeReminderTime),
          ),
        ),
        if (state.permissionDenied)
          Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Text(l10n.notificationPermissionDenied),
          ),
      ],
    );
  }

  Future<void> _pickTime(BuildContext context, WidgetRef ref) async {
    final picked = await showTimePicker(
      context: context,
      initialTime: state.reminderTime,
    );
    if (picked != null) {
      ref.read(onboardingControllerProvider.notifier).setReminderTime(picked);
    }
  }
}
