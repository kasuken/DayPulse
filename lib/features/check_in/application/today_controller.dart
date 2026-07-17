import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/utilities/date_utils.dart';
import '../data/drift_activity_repository.dart';
import '../data/drift_check_in_repository.dart';
import '../domain/activity.dart';
import '../domain/check_in.dart';

/// Streams today's check-in entry.
final todayCheckInProvider = StreamProvider.autoDispose<CheckIn?>((ref) {
  final today = DateUtilsX.dateOnly(DateTime.now());
  return ref.watch(checkInRepositoryProvider).watchByDate(today);
});

/// Streams active activities for Today screen display.
final todayActivitiesProvider = StreamProvider.autoDispose<List<Activity>>((
  ref,
) {
  return ref.watch(activityRepositoryProvider).watchActive();
});
