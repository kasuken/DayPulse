import 'package:flutter/material.dart';

/// Brand color tokens for DayPulse.
///
/// Widgets must consume colors through [ColorScheme] or these tokens,
/// never hard-coded hex values.
abstract final class AppColors {
  static const Color primary = Color(0xFF6C63FF);
  static const Color secondary = Color(0xFF55C2DA);
  static const Color success = Color(0xFF57C46A);
  static const Color warning = Color(0xFFF7B955);
  static const Color error = Color(0xFFE95F5F);
}
