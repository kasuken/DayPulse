import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:uuid/uuid.dart';

import '../../../core/errors/app_exception.dart';
import '../../../core/utilities/date_utils.dart';
import '../../settings/application/settings_controller.dart';
import '../../settings/domain/wellbeing_settings.dart';
import '../data/drift_activity_repository.dart';
import '../data/drift_check_in_repository.dart';
import '../domain/activity.dart';
import '../domain/check_in.dart';
import '../domain/check_in_validator.dart';

/// Loads the initial state for a check-in form.
final checkInFormProvider = FutureProvider.autoDispose
    .family<CheckInFormState, String?>((ref, id) async {
      final repository = ref.watch(checkInRepositoryProvider);
      final metrics =
          ref.watch(settingsControllerProvider).value?.metrics ??
          const WellbeingSettings();
      if (id == null) {
        return CheckInFormState.create(metrics: metrics);
      }
      final entry = await repository.getById(id);
      if (entry == null) {
        throw const DatabaseFailure('Check-in not found');
      }
      return CheckInFormState.fromEntry(entry, metrics: metrics);
    });

/// Provides form commands for saving entries and creating activities.
final checkInFormControllerProvider = Provider<CheckInFormController>((ref) {
  return CheckInFormController(ref);
});

/// Streams active activities for the check-in form.
final checkInFormActivitiesProvider =
    StreamProvider.autoDispose<List<Activity>>((ref) {
      return ref.watch(activityRepositoryProvider).watchActive();
    });

/// Mutable field values for the check-in form.
class CheckInFormState {
  /// Creates form state.
  const CheckInFormState({
    required this.id,
    required this.entryDate,
    required this.mood,
    required this.energy,
    required this.anxiety,
    required this.focus,
    required this.sleepHours,
    required this.activityIds,
    required this.notes,
    required this.createdAt,
    required this.metrics,
  });

  /// Creates default state for a new entry.
  factory CheckInFormState.create({required WellbeingSettings metrics}) =>
      CheckInFormState(
        id: null,
        entryDate: DateUtilsX.dateOnly(DateTime.now()),
        mood: 0,
        energy: 5,
        anxiety: 5,
        focus: 5,
        sleepHours: 7,
        activityIds: const [],
        notes: '',
        createdAt: null,
        metrics: metrics,
      );

  /// Creates edit state from an existing entry.
  factory CheckInFormState.fromEntry(
    CheckIn entry, {
    required WellbeingSettings metrics,
  }) => CheckInFormState(
    id: entry.id,
    entryDate: DateUtilsX.dateOnly(entry.entryDate),
    mood: entry.mood,
    energy: entry.energy ?? 5,
    anxiety: entry.anxiety ?? 5,
    focus: entry.focus ?? 5,
    sleepHours: entry.sleepHours ?? 7,
    activityIds: entry.activityIds,
    notes: entry.notes ?? '',
    createdAt: entry.createdAt,
    metrics: metrics,
  );

  /// Entry id in edit mode.
  final String? id;

  /// Local calendar date for the entry.
  final DateTime entryDate;

  /// Mood value from -5 to 5.
  final int mood;

  /// Energy value from 0 to 10.
  final int energy;

  /// Anxiety value from 0 to 10.
  final int anxiety;

  /// Focus value from 0 to 10.
  final int focus;

  /// Sleep duration in hours.
  final double sleepHours;

  /// Selected activity ids.
  final List<String> activityIds;

  /// Optional notes.
  final String notes;

  /// Original creation timestamp in edit mode.
  final DateTime? createdAt;

  /// Enabled optional metrics.
  final WellbeingSettings metrics;

  /// Returns a modified copy.
  CheckInFormState copyWith({
    DateTime? entryDate,
    int? mood,
    int? energy,
    int? anxiety,
    int? focus,
    double? sleepHours,
    List<String>? activityIds,
    String? notes,
  }) => CheckInFormState(
    id: id,
    entryDate: entryDate ?? this.entryDate,
    mood: mood ?? this.mood,
    energy: energy ?? this.energy,
    anxiety: anxiety ?? this.anxiety,
    focus: focus ?? this.focus,
    sleepHours: sleepHours ?? this.sleepHours,
    activityIds: activityIds ?? this.activityIds,
    notes: notes ?? this.notes,
    createdAt: createdAt,
    metrics: metrics,
  );

  /// Converts form fields to a domain check-in.
  CheckIn toCheckIn({required DateTime now}) => CheckIn(
    id: id ?? const Uuid().v4(),
    entryDate: entryDate,
    mood: mood,
    energy: metrics.energyEnabled ? energy : null,
    anxiety: metrics.anxietyEnabled ? anxiety : null,
    focus: metrics.focusEnabled ? focus : null,
    sleepHours: metrics.sleepEnabled ? sleepHours : null,
    activityIds: activityIds,
    notes: notes.trim().isEmpty ? null : notes.trim(),
    createdAt: createdAt ?? now,
    updatedAt: now,
  );
}

/// Result of attempting to save a check-in.
enum SaveCheckInResult {
  /// Save completed.
  saved,

  /// Form values failed validation.
  invalid,

  /// Another entry already exists for the selected date.
  duplicateDate,

  /// The save failed due to persistence problems.
  failed,
}

/// Application service for check-in form commands.
class CheckInFormController {
  /// Creates the form controller.
  const CheckInFormController(this._ref);

  final Ref _ref;

  /// Saves a form state using create or update as appropriate.
  Future<SaveCheckInResult> save(CheckInFormState form) async {
    final now = DateTime.now().toUtc();
    final checkIn = form.toCheckIn(now: now);
    final violations = CheckInValidator.validate(checkIn);
    if (violations.isNotEmpty) {
      return SaveCheckInResult.invalid;
    }
    try {
      if (form.id == null) {
        await _ref.read(checkInRepositoryProvider).create(checkIn);
      } else {
        await _ref.read(checkInRepositoryProvider).update(checkIn);
      }
      return SaveCheckInResult.saved;
    } on DuplicateEntryDateFailure {
      return SaveCheckInResult.duplicateDate;
    } on DatabaseFailure {
      return SaveCheckInResult.failed;
    } on AppException {
      return SaveCheckInResult.failed;
    }
  }

  /// Creates a custom activity after the current last sort order.
  Future<void> addActivity(String name) async {
    final repository = _ref.read(activityRepositoryProvider);
    final active = await repository.getActive();
    final nextSortOrder = active.isEmpty
        ? 0
        : active
                  .map((activity) => activity.sortOrder)
                  .reduce((a, b) => a > b ? a : b) +
              1;
    await repository.add(
      Activity(
        id: const Uuid().v4(),
        name: name.trim(),
        isDefault: false,
        isActive: true,
        sortOrder: nextSortOrder,
      ),
    );
  }
}
