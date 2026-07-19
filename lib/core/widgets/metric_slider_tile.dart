import 'package:flutter/material.dart';

import '../../app/theme/app_spacing.dart';
import '../../app/theme/app_typography.dart';
import '../../l10n/app_localizations.dart';

/// Reusable labeled slider tile for wellbeing metrics.
class MetricSliderTile extends StatelessWidget {
  /// Creates a metric slider tile.
  const MetricSliderTile({
    required this.title,
    required this.value,
    required this.min,
    required this.max,
    required this.divisions,
    required this.displayValue,
    required this.onChanged,
    this.semanticValue,
    this.helperText,
    this.showRangeLabels = true,
    this.lowLabel,
    this.moderateLabel,
    this.highLabel,
    super.key,
  });

  /// Metric title.
  final String title;

  /// Current slider value.
  final double value;

  /// Minimum slider value.
  final double min;

  /// Maximum slider value.
  final double max;

  /// Slider divisions.
  final int divisions;

  /// Displayed value.
  final String displayValue;

  /// Accessible value override.
  final String? semanticValue;

  /// Optional helper text.
  final String? helperText;

  /// Whether to show low/moderate/high range labels.
  final bool showRangeLabels;

  /// Optional low range label.
  final String? lowLabel;

  /// Optional moderate range label.
  final String? moderateLabel;

  /// Optional high range label.
  final String? highLabel;

  /// Called when the slider changes.
  final ValueChanged<double> onChanged;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final effectiveLowLabel = lowLabel ?? l10n.metricLow;
    final effectiveModerateLabel = moderateLabel ?? l10n.metricModerate;
    final effectiveHighLabel = highLabel ?? l10n.metricHigh;
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    title,
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: AppSpacing.sm,
                    vertical: AppSpacing.xxs,
                  ),
                  decoration: BoxDecoration(
                    color: Theme.of(context).colorScheme.secondaryContainer,
                    borderRadius: BorderRadius.circular(AppRadius.small),
                  ),
                  child: Text(
                    displayValue,
                    style: AppTypography.metricValue(context)?.copyWith(
                      color: Theme.of(
                        context,
                      ).colorScheme.onSecondaryContainer,
                    ),
                  ),
                ),
              ],
            ),
            if (helperText != null) ...[
              const SizedBox(height: AppSpacing.xs),
              Text(helperText!),
            ],
            Slider(
              value: value,
              min: min,
              max: max,
              divisions: divisions,
              label: displayValue,
              semanticFormatterCallback: (sliderValue) =>
                  l10n.metricValueLabel(title, semanticValue ?? displayValue),
              onChanged: onChanged,
            ),
            if (showRangeLabels)
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    effectiveLowLabel,
                    style: AppTypography.eyebrow(context),
                  ),
                  Text(
                    effectiveModerateLabel,
                    style: AppTypography.eyebrow(context),
                  ),
                  Text(
                    effectiveHighLabel,
                    style: AppTypography.eyebrow(context),
                  ),
                ],
              ),
          ],
        ),
      ),
    );
  }
}
