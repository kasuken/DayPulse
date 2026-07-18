import 'package:flutter/material.dart';

import 'app_colors.dart';
import 'app_spacing.dart';
import 'app_typography.dart';

/// Builds Material 3 light and dark themes for DayPulse.
///
/// Component shapes follow one rule: containers that hold a day's story
/// (cards, sheets) are soft and generous; controls the user acts on
/// (buttons, chips, inputs) are tighter and more precise.
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
      scaffoldBackgroundColor: scheme.surface,
      appBarTheme: theme.appBarTheme.copyWith(
        backgroundColor: scheme.surface,
        surfaceTintColor: Colors.transparent,
        elevation: 0,
        centerTitle: false,
        titleTextStyle: AppTypography.textTheme(
          theme.textTheme,
        ).headlineSmall?.copyWith(color: scheme.onSurface),
      ),
      cardTheme: theme.cardTheme.copyWith(
        elevation: 0,
        color: scheme.surfaceContainerLow,
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
          textStyle: theme.textTheme.labelLarge?.copyWith(
            fontWeight: FontWeight.w700,
            letterSpacing: 0.25,
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          minimumSize: const Size.fromHeight(AppTouchTarget.minimum),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppRadius.medium),
          ),
          side: BorderSide(color: scheme.outlineVariant),
        ),
      ),
      chipTheme: theme.chipTheme.copyWith(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadius.small),
        ),
        side: BorderSide(color: scheme.outlineVariant),
      ),
      inputDecorationTheme: theme.inputDecorationTheme.copyWith(
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(AppRadius.medium),
        ),
        filled: true,
        fillColor: scheme.surfaceContainerLow,
      ),
      navigationBarTheme: theme.navigationBarTheme.copyWith(
        backgroundColor: scheme.surface,
        surfaceTintColor: Colors.transparent,
        indicatorColor: scheme.secondaryContainer,
        elevation: 0,
      ),
      sliderTheme: theme.sliderTheme.copyWith(
        trackHeight: 6,
        activeTrackColor: scheme.primary,
        inactiveTrackColor: scheme.surfaceContainerHighest,
      ),
      dividerTheme: theme.dividerTheme.copyWith(
        color: scheme.outlineVariant,
        thickness: 1,
        space: AppSpacing.lg,
      ),
      snackBarTheme: theme.snackBarTheme.copyWith(
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadius.medium),
        ),
      ),
    );
  }
}
