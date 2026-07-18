import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../../app/theme/app_spacing.dart';
import '../../../core/widgets/metric_slider_tile.dart';
import '../../../l10n/app_localizations.dart';
import '../application/check_in_form_controller.dart';
import '../domain/activity.dart';
import '../domain/check_in_validator.dart';
import 'mood_labels.dart';

/// Create and edit form for a daily check-in.
class CheckInScreen extends ConsumerStatefulWidget {
  /// Creates a check-in form. When [id] is null the form creates a new entry.
  const CheckInScreen({this.id, super.key});

  /// Existing check-in id for edit mode.
  final String? id;

  @override
  ConsumerState<CheckInScreen> createState() => _CheckInScreenState();
}

class _CheckInScreenState extends ConsumerState<CheckInScreen> {
  final _notesController = TextEditingController();
  CheckInFormState? _form;
  bool _saving = false;

  @override
  void dispose() {
    _notesController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final formValue = ref.watch(checkInFormProvider(widget.id));
    final activities =
        ref.watch(checkInFormActivitiesProvider).value ?? const <Activity>[];
    return Scaffold(
      appBar: AppBar(
        title: Text(
          widget.id == null ? l10n.checkInTitleCreate : l10n.checkInTitleEdit,
        ),
      ),
      body: formValue.when(
        data: (loadedForm) {
          _ensureForm(loadedForm);
          final form = _form!;
          return ListView(
            padding: const EdgeInsets.all(AppSpacing.md),
            children: [
              _DateTile(date: form.entryDate, onPick: () => _pickDate(form)),
              const SizedBox(height: AppSpacing.md),
              _MoodCard(
                value: form.mood,
                onChanged: (value) => _update(form.copyWith(mood: value)),
              ),
              if (form.metrics.energyEnabled)
                MetricSliderTile(
                  title: l10n.metricEnergy,
                  value: form.energy.toDouble(),
                  min: 0,
                  max: 10,
                  divisions: 10,
                  displayValue: form.energy.toString(),
                  onChanged: (value) =>
                      _update(form.copyWith(energy: value.round())),
                ),
              if (form.metrics.anxietyEnabled)
                MetricSliderTile(
                  title: l10n.metricAnxiety,
                  value: form.anxiety.toDouble(),
                  min: 0,
                  max: 10,
                  divisions: 10,
                  displayValue: form.anxiety.toString(),
                  helperText: l10n.anxietyLowerPreferable,
                  onChanged: (value) =>
                      _update(form.copyWith(anxiety: value.round())),
                ),
              if (form.metrics.focusEnabled)
                MetricSliderTile(
                  title: l10n.metricFocus,
                  value: form.focus.toDouble(),
                  min: 0,
                  max: 10,
                  divisions: 10,
                  displayValue: form.focus.toString(),
                  onChanged: (value) =>
                      _update(form.copyWith(focus: value.round())),
                ),
              if (form.metrics.sleepEnabled)
                MetricSliderTile(
                  title: l10n.metricSleep,
                  value: form.sleepHours,
                  min: 0,
                  max: 15,
                  divisions: 30,
                  displayValue: l10n.sleepHoursValue(
                    _formatHours(form.sleepHours),
                  ),
                  showRangeLabels: false,
                  onChanged: (value) =>
                      _update(form.copyWith(sleepHours: value)),
                ),
              _ActivitiesSection(
                activities: activities,
                selectedIds: form.activityIds,
                onChanged: (ids) => _update(form.copyWith(activityIds: ids)),
                onAdd: _addActivity,
              ),
              const SizedBox(height: AppSpacing.md),
              TextField(
                controller: _notesController,
                maxLength: CheckInValidator.maxNotesLength,
                maxLines: 4,
                autofocus: false,
                decoration: InputDecoration(
                  labelText: l10n.notesLabel,
                  hintText: l10n.notesHint,
                ),
                onChanged: (value) => _update(form.copyWith(notes: value)),
              ),
              const SizedBox(height: AppSpacing.lg),
              FilledButton(
                onPressed: _saving ? null : () => _save(form),
                child: Text(_saving ? l10n.savingCheckIn : l10n.saveCheckIn),
              ),
            ],
          );
        },
        loading: () => const SizedBox.shrink(),
        error: (error, stackTrace) => Center(
          child: Padding(
            padding: const EdgeInsets.all(AppSpacing.lg),
            child: Text(
              widget.id == null
                  ? l10n.loadCheckInFailureMessage
                  : l10n.checkInNotFoundMessage,
            ),
          ),
        ),
      ),
    );
  }

  void _ensureForm(CheckInFormState loadedForm) {
    if (_form != null) {
      return;
    }
    _form = loadedForm;
    _notesController.text = loadedForm.notes;
  }

  void _update(CheckInFormState form) {
    setState(() => _form = form);
  }

  Future<void> _pickDate(CheckInFormState form) async {
    final now = DateTime.now();
    final picked = await showDatePicker(
      context: context,
      initialDate: form.entryDate,
      firstDate: DateTime(now.year - 10),
      lastDate: DateTime(now.year, now.month, now.day),
    );
    if (picked != null) {
      _update(
        form.copyWith(
          entryDate: DateTime(picked.year, picked.month, picked.day),
        ),
      );
    }
  }

  Future<void> _addActivity() async {
    final l10n = AppLocalizations.of(context)!;
    final name = await showDialog<String>(
      context: context,
      builder: (context) => const _AddActivityDialog(),
    );
    if (name == null || name.trim().isEmpty) {
      return;
    }
    try {
      await ref.read(checkInFormControllerProvider).addActivity(name);
    } on Object {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text(l10n.saveCheckInFailureMessage)));
      }
    }
  }

  Future<void> _save(CheckInFormState form) async {
    final l10n = AppLocalizations.of(context)!;
    setState(() => _saving = true);
    final result = await ref.read(checkInFormControllerProvider).save(form);
    if (!mounted) {
      return;
    }
    setState(() => _saving = false);
    switch (result) {
      case SaveCheckInResult.saved:
        await HapticFeedback.mediumImpact();
        if (!mounted) {
          return;
        }
        if (context.canPop()) {
          context.pop();
        } else {
          context.go('/today');
        }
      case SaveCheckInResult.invalid:
        _showMessage(l10n.checkInInvalidMessage);
      case SaveCheckInResult.duplicateDate:
        _showMessage(l10n.duplicateEntryDateMessage);
      case SaveCheckInResult.failed:
        _showMessage(l10n.saveCheckInFailureMessage);
    }
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message)));
  }

  String _formatHours(double hours) =>
      hours.toStringAsFixed(hours % 1 == 0 ? 0 : 1);
}

class _DateTile extends StatelessWidget {
  const _DateTile({required this.date, required this.onPick});

  final DateTime date;
  final VoidCallback onPick;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final locale = Localizations.localeOf(context).toString();
    return Card(
      child: ListTile(
        title: Text(l10n.entryDateLabel),
        subtitle: Text(DateFormat.yMMMMEEEEd(locale).format(date)),
        trailing: OutlinedButton(
          onPressed: onPick,
          child: Text(l10n.changeDate),
        ),
      ),
    );
  }
}

class _MoodCard extends StatelessWidget {
  const _MoodCard({required this.value, required this.onChanged});

  final int value;
  final ValueChanged<int> onChanged;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final label = moodLabel(l10n, value);
    final display = l10n.moodValueLabel(value, label);
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              l10n.metricMood,
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: AppSpacing.sm),
            Row(
              children: [
                Text(
                  moodEmoji(value),
                  style: Theme.of(context).textTheme.displaySmall,
                ),
                const SizedBox(width: AppSpacing.md),
                Expanded(
                  child: Text(
                    display,
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                ),
              ],
            ),
            Slider(
              value: value.toDouble(),
              min: -5,
              max: 5,
              divisions: 10,
              label: display,
              semanticFormatterCallback: (sliderValue) => l10n.moodValueLabel(
                sliderValue.round(),
                moodLabel(l10n, sliderValue.round()),
              ),
              onChanged: (sliderValue) => onChanged(sliderValue.round()),
            ),
          ],
        ),
      ),
    );
  }
}

class _ActivitiesSection extends StatelessWidget {
  const _ActivitiesSection({
    required this.activities,
    required this.selectedIds,
    required this.onChanged,
    required this.onAdd,
  });

  final List<Activity> activities;
  final List<String> selectedIds;
  final ValueChanged<List<String>> onChanged;
  final VoidCallback onAdd;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.md),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              l10n.activitiesLabel,
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: AppSpacing.sm),
            Wrap(
              spacing: AppSpacing.sm,
              runSpacing: AppSpacing.xs,
              children: [
                for (final activity in activities)
                  FilterChip(
                    label: Text(activity.name),
                    selected: selectedIds.contains(activity.id),
                    onSelected: (selected) {
                      final next = selectedIds.toList();
                      if (selected) {
                        next.add(activity.id);
                      } else {
                        next.remove(activity.id);
                      }
                      onChanged(next.toSet().toList(growable: false));
                    },
                  ),
                ActionChip(
                  avatar: const Icon(Icons.add),
                  label: Text(l10n.addActivity),
                  onPressed: onAdd,
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _AddActivityDialog extends StatefulWidget {
  const _AddActivityDialog();

  @override
  State<_AddActivityDialog> createState() => _AddActivityDialogState();
}

class _AddActivityDialogState extends State<_AddActivityDialog> {
  final _controller = TextEditingController();

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return AlertDialog(
      title: Text(l10n.newActivityTitle),
      content: TextField(
        controller: _controller,
        decoration: InputDecoration(labelText: l10n.activityNameLabel),
        textInputAction: TextInputAction.done,
        onSubmitted: (_) => _submit(),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text(l10n.cancel),
        ),
        FilledButton(onPressed: _submit, child: Text(l10n.add)),
      ],
    );
  }

  void _submit() {
    Navigator.of(context).pop(_controller.text.trim());
  }
}
