import 'check_in.dart';

/// Persistence contract for check-in entries.
abstract interface class CheckInRepository {
  /// Returns every check-in ordered by entry date descending.
  Future<List<CheckIn>> getAll();

  /// Returns check-ins between [start] and [end], inclusive.
  Future<List<CheckIn>> getBetween(DateTime start, DateTime end);

  /// Returns the check-in with [id], or null when missing.
  Future<CheckIn?> getById(String id);

  /// Returns the check-in for local calendar [date], or null when missing.
  Future<CheckIn?> getByDate(DateTime date);

  /// Watches the check-in for local calendar [date].
  Stream<CheckIn?> watchByDate(DateTime date);

  /// Watches every check-in ordered by entry date descending.
  Stream<List<CheckIn>> watchAll();

  /// Creates [checkIn].
  Future<void> create(CheckIn checkIn);

  /// Updates [checkIn], preserving its original creation timestamp.
  Future<void> update(CheckIn checkIn);

  /// Deletes a check-in by [id].
  Future<void> delete(String id);

  /// Deletes all check-ins and their activity links.
  Future<void> deleteAll();
}
