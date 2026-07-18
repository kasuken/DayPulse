import 'package:flutter/material.dart';

/// Typography helpers built on Material 3 text styles.
///
/// DayPulse ships no bundled display face (the app is offline-first and
/// never fetches fonts), so personality comes from precision: heavy,
/// tightly-tracked display styles, tracked uppercase eyebrows, and tabular
/// figures wherever metric numerals need to align.
abstract final class AppTypography {
  /// Tabular figures so metric numerals align in rows and tables.
  static const List<FontFeature> tabularFigures = <FontFeature>[
    FontFeature.tabularFigures(),
  ];

  /// Base text theme; relies on Material 3 defaults for dynamic scaling.
  static TextTheme textTheme(TextTheme base) {
    return base.copyWith(
      displaySmall: base.displaySmall?.copyWith(
        fontWeight: FontWeight.w800,
        letterSpacing: -1.0,
        height: 1.1,
      ),
      headlineMedium: base.headlineMedium?.copyWith(
        fontWeight: FontWeight.w800,
        letterSpacing: -0.5,
        height: 1.2,
      ),
      headlineSmall: base.headlineSmall?.copyWith(
        fontWeight: FontWeight.w700,
        letterSpacing: -0.25,
        height: 1.25,
      ),
      titleLarge: base.titleLarge?.copyWith(
        fontWeight: FontWeight.w700,
        height: 1.3,
      ),
      bodyMedium: base.bodyMedium?.copyWith(height: 1.45),
      labelLarge: base.labelLarge?.copyWith(fontWeight: FontWeight.w600),
      labelSmall: base.labelSmall?.copyWith(
        fontWeight: FontWeight.w600,
        letterSpacing: 1.2,
      ),
    );
  }

  /// Eyebrow style for small tracked-uppercase context labels.
  static TextStyle? eyebrow(BuildContext context) {
    final theme = Theme.of(context);
    return theme.textTheme.labelSmall?.copyWith(
      color: theme.colorScheme.onSurfaceVariant,
    );
  }

  /// Numeric style with tabular figures for metric values.
  static TextStyle? metricValue(BuildContext context) {
    final theme = Theme.of(context);
    return theme.textTheme.titleMedium?.copyWith(
      fontWeight: FontWeight.w700,
      fontFeatures: tabularFigures,
    );
  }
}
