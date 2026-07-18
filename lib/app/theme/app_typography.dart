import 'package:flutter/material.dart';

/// Typography helpers built on Material 3 text styles.
abstract final class AppTypography {
  /// Base text theme; relies on Material 3 defaults for dynamic scaling.
  static TextTheme textTheme(TextTheme base) {
    return base.copyWith(
      headlineMedium: base.headlineMedium?.copyWith(
        fontWeight: FontWeight.w700,
        height: 1.25,
      ),
      titleLarge: base.titleLarge?.copyWith(
        fontWeight: FontWeight.w600,
        height: 1.3,
      ),
      bodyMedium: base.bodyMedium?.copyWith(height: 1.4),
      labelLarge: base.labelLarge?.copyWith(fontWeight: FontWeight.w600),
    );
  }
}
