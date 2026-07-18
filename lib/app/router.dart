import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../features/check_in/presentation/check_in_screen.dart';
import '../features/check_in/presentation/today_screen.dart';
import '../features/history/presentation/entry_detail_screen.dart';
import '../features/history/presentation/history_screen.dart';
import '../features/insights/presentation/insights_screen.dart';
import '../features/onboarding/presentation/onboarding_screen.dart';
import '../features/settings/application/settings_controller.dart';
import '../features/settings/presentation/activity_management_screen.dart';
import '../features/settings/presentation/appearance_settings_screen.dart';
import '../features/settings/presentation/privacy_settings_screen.dart';
import '../features/settings/presentation/reminder_settings_screen.dart';
import '../features/settings/presentation/settings_screen.dart';
import '../l10n/app_localizations.dart';

final _rootNavigatorKey = GlobalKey<NavigatorState>();
final _shellNavigatorTodayKey = GlobalKey<NavigatorState>();
final _shellNavigatorHistoryKey = GlobalKey<NavigatorState>();
final _shellNavigatorInsightsKey = GlobalKey<NavigatorState>();
final _shellNavigatorSettingsKey = GlobalKey<NavigatorState>();

/// Provides the application router.
final appRouterProvider = Provider<GoRouter>((ref) {
  final settings = ref.watch(settingsControllerProvider);
  return GoRouter(
    navigatorKey: _rootNavigatorKey,
    initialLocation: '/today',
    redirect: (context, state) {
      final loaded = settings.value;
      if (loaded == null) {
        return null;
      }
      final location = state.uri.path;
      if (!loaded.onboardingCompleted && location != '/onboarding') {
        return '/onboarding';
      }
      if (loaded.onboardingCompleted && location == '/onboarding') {
        return '/today';
      }
      return null;
    },
    routes: [
      GoRoute(
        path: '/onboarding',
        builder: (context, state) => const OnboardingScreen(),
      ),
      StatefulShellRoute.indexedStack(
        builder: (context, state, navigationShell) =>
            _AppScaffold(navigationShell: navigationShell),
        branches: [
          StatefulShellBranch(
            navigatorKey: _shellNavigatorTodayKey,
            routes: [
              GoRoute(
                path: '/today',
                builder: (context, state) => const TodayScreen(),
              ),
            ],
          ),
          StatefulShellBranch(
            navigatorKey: _shellNavigatorHistoryKey,
            routes: [
              GoRoute(
                path: '/history',
                builder: (context, state) => const HistoryScreen(),
              ),
            ],
          ),
          StatefulShellBranch(
            navigatorKey: _shellNavigatorInsightsKey,
            routes: [
              GoRoute(
                path: '/insights',
                builder: (context, state) => const InsightsScreen(),
              ),
            ],
          ),
          StatefulShellBranch(
            navigatorKey: _shellNavigatorSettingsKey,
            routes: [
              GoRoute(
                path: '/settings',
                builder: (context, state) => const SettingsScreen(),
                routes: [
                  GoRoute(
                    path: 'appearance',
                    builder: (context, state) =>
                        const AppearanceSettingsScreen(),
                  ),
                  GoRoute(
                    path: 'reminders',
                    builder: (context, state) => const ReminderSettingsScreen(),
                  ),
                  GoRoute(
                    path: 'privacy',
                    builder: (context, state) => const PrivacySettingsScreen(),
                  ),
                  GoRoute(
                    path: 'activities',
                    builder: (context, state) =>
                        const ActivityManagementScreen(),
                  ),
                ],
              ),
            ],
          ),
        ],
      ),
      GoRoute(
        parentNavigatorKey: _rootNavigatorKey,
        path: '/check-in',
        builder: (context, state) => const CheckInScreen(),
      ),
      GoRoute(
        parentNavigatorKey: _rootNavigatorKey,
        path: '/check-in/:id',
        builder: (context, state) =>
            CheckInScreen(id: state.pathParameters['id']),
      ),
      GoRoute(
        parentNavigatorKey: _rootNavigatorKey,
        path: '/history/:id',
        builder: (context, state) =>
            EntryDetailScreen(id: state.pathParameters['id']!),
      ),
    ],
  );
});

class _AppScaffold extends StatelessWidget {
  const _AppScaffold({required this.navigationShell});

  final StatefulNavigationShell navigationShell;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      body: navigationShell,
      bottomNavigationBar: NavigationBar(
        selectedIndex: navigationShell.currentIndex,
        onDestinationSelected: (index) => navigationShell.goBranch(
          index,
          initialLocation: index == navigationShell.currentIndex,
        ),
        destinations: [
          NavigationDestination(
            icon: const Icon(Icons.today_outlined),
            selectedIcon: const Icon(Icons.today),
            label: l10n.todayTabLabel,
            tooltip: l10n.todayTabLabel,
          ),
          NavigationDestination(
            icon: const Icon(Icons.history_outlined),
            selectedIcon: const Icon(Icons.history),
            label: l10n.historyTabLabel,
            tooltip: l10n.historyTabLabel,
          ),
          NavigationDestination(
            icon: const Icon(Icons.insights_outlined),
            selectedIcon: const Icon(Icons.insights),
            label: l10n.insightsTabLabel,
            tooltip: l10n.insightsTabLabel,
          ),
          NavigationDestination(
            icon: const Icon(Icons.settings_outlined),
            selectedIcon: const Icon(Icons.settings),
            label: l10n.settingsTabLabel,
            tooltip: l10n.settingsTabLabel,
          ),
        ],
      ),
    );
  }
}
