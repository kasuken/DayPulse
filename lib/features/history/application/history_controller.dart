import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../check_in/data/drift_activity_repository.dart';
import '../../check_in/data/drift_check_in_repository.dart';
import '../../check_in/domain/activity.dart';
import '../../check_in/domain/check_in.dart';

/// Streams all check-ins for the History screen.
final historyEntriesProvider = StreamProvider.autoDispose<List<CheckIn>>((ref) {
  return ref.watch(checkInRepositoryProvider).watchAll();
});

/// Streams known active activities for history display.
final historyActivitiesProvider = StreamProvider.autoDispose<List<Activity>>((
  ref,
) {
  return ref.watch(activityRepositoryProvider).watchActive();
});

/// Loads one check-in by id for the Entry Detail screen.
final historyEntryProvider = FutureProvider.autoDispose
    .family<CheckIn?, String>((ref, id) {
      return ref.watch(checkInRepositoryProvider).getById(id);
    });

/// Commands for check-in history entries.
final historyControllerProvider = Provider<HistoryController>((ref) {
  return HistoryController(ref);
});

/// Application controller for History actions.
class HistoryController {
  /// Creates a history controller.
  const HistoryController(this._ref);

  final Ref _ref;

  /// Deletes a check-in entry.
  Future<void> deleteEntry(String id) =>
      _ref.read(checkInRepositoryProvider).delete(id);
}
