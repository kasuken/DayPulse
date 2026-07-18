import 'package:flutter/material.dart';

/// DayPulse's signature mark: a single ECG-style beat drawn as a line.
///
/// The beat's amplitude follows a normalized intensity (0–1), so the mark
/// quietly encodes today's mood rather than decorating the page. It is
/// intentionally static — no animation — so it respects reduced motion by
/// default and stays calm, like the app.
class PulseLine extends StatelessWidget {
  /// Creates a pulse line.
  const PulseLine({
    this.intensity = 0.6,
    this.height = 28,
    this.color,
    super.key,
  });

  /// Normalized beat amplitude from 0 (flat, low) to 1 (full beat).
  final double intensity;

  /// Height of the drawing area in logical pixels.
  final double height;

  /// Stroke color; defaults to the theme's secondary color — the ember
  /// accent reserved for the pulse.
  final Color? color;

  @override
  Widget build(BuildContext context) {
    final strokeColor = color ?? Theme.of(context).colorScheme.secondary;
    return ExcludeSemantics(
      child: SizedBox(
        height: height,
        width: double.infinity,
        child: CustomPaint(
          painter: _PulseLinePainter(
            intensity: intensity.clamp(0.0, 1.0),
            color: strokeColor,
          ),
        ),
      ),
    );
  }
}

class _PulseLinePainter extends CustomPainter {
  const _PulseLinePainter({required this.intensity, required this.color});

  final double intensity;
  final Color color;

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2
      ..strokeCap = StrokeCap.round
      ..strokeJoin = StrokeJoin.round;

    final midY = size.height / 2;
    // Keep a minimum visible beat so a low day still registers as alive.
    final amplitude = (size.height / 2 - 2) * (0.25 + 0.75 * intensity);
    final beatStart = size.width * 0.35;
    final beatWidth = size.width * 0.3;

    final path = Path()
      ..moveTo(0, midY)
      ..lineTo(beatStart, midY)
      ..lineTo(beatStart + beatWidth * 0.15, midY - amplitude * 0.35)
      ..lineTo(beatStart + beatWidth * 0.35, midY + amplitude * 0.45)
      ..lineTo(beatStart + beatWidth * 0.55, midY - amplitude)
      ..lineTo(beatStart + beatWidth * 0.75, midY + amplitude * 0.55)
      ..lineTo(beatStart + beatWidth * 0.9, midY - amplitude * 0.2)
      ..lineTo(beatStart + beatWidth, midY)
      ..lineTo(size.width, midY);
    canvas.drawPath(path, paint);
  }

  @override
  bool shouldRepaint(_PulseLinePainter oldDelegate) =>
      oldDelegate.intensity != intensity || oldDelegate.color != color;
}
