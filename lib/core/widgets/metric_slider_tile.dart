import 'package:flutter/material.dart';

import '../../app/theme/app_spacing.dart';
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

  /// Called when the slider changes.
  final ValueChanged<double> onChanged;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
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
                Text(
                  displayValue,
                  style: Theme.of(context).textTheme.titleMedium,
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
                  Text(l10n.metricLow),
                  Text(l10n.metricModerate),
                  Text(l10n.metricHigh),
                ],
              ),
          ],
        ),
      ),
    );
  }
}
