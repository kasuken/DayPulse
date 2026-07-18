import 'package:flutter/material.dart';

import 'app_colors.dart';
import 'app_spacing.dart';
import 'app_typography.dart';

/// Builds Material 3 light and dark themes for DayPulse.
abstract final class AppTheme {
  static ThemeData light([ColorScheme? dynamicScheme]) {
    final scheme =
        dynamicScheme ??
        ColorScheme.fromSeed(
          seedColor: AppColors.primary,
          secondary: AppColors.secondary,
          error: AppColors.error,
        );
    return _base(scheme);
  }

  static ThemeData dark([ColorScheme? dynamicScheme]) {
    final scheme =
        dynamicScheme ??
        ColorScheme.fromSeed(
          seedColor: AppColors.primary,
          secondary: AppColors.secondary,
          error: AppColors.error,
          brightness: Brightness.dark,
        );
    return _base(scheme);
  }

  static ThemeData _base(ColorScheme scheme) {
    final theme = ThemeData(colorScheme: scheme, useMaterial3: true);
    return theme.copyWith(
      textTheme: AppTypography.textTheme(theme.textTheme),
      cardTheme: theme.cardTheme.copyWith(
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadius.large),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          minimumSize: const Size.fromHeight(AppTouchTarget.minimum + 8),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppRadius.medium),
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          minimumSize: const Size.fromHeight(AppTouchTarget.minimum),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppRadius.medium),
          ),
        ),
      ),
      chipTheme: theme.chipTheme.copyWith(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadius.small),
        ),
      ),
      inputDecorationTheme: theme.inputDecorationTheme.copyWith(
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(AppRadius.medium),
        ),
      ),
    );
  }
}
