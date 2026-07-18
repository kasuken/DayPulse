import 'package:flutter/material.dart';

/// Brand color tokens for DayPulse.
///
/// The palette is circadian: a calm "dusk water" teal field with a single
/// warm ember accent reserved for the pulse — the vital sign at the heart
/// of the app. Widgets must consume colors through [ColorScheme] or these
/// tokens, never hard-coded hex values.
abstract final class AppColors {
  /// Petrol teal — early-evening water; the calm field everything sits on.
  static const Color primary = Color(0xFF175E63);

  /// Warm ember — the color of the pulse itself; spent sparingly.
  static const Color secondary = Color(0xFFE0784A);

  /// Sage — a settled, steady day.
  static const Color success = Color(0xFF5F8D6B);

  /// Ochre — attention without alarm.
  static const Color warning = Color(0xFFD9962E);

  /// Clay — errors stay warm-toned, never harsh.
  static const Color error = Color(0xFFC05B4D);
}
