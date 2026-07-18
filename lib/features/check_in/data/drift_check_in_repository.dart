import 'package:drift/drift.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/database/app_database.dart';
import '../../../core/database/database_provider.dart';
import '../../../core/errors/app_exception.dart';
import '../../../core/utilities/date_utils.dart';
import '../domain/check_in.dart' as domain;
import '../domain/check_in_repository.dart';
import '../domain/check_in_validator.dart';

/// Drift-backed implementation of [CheckInRepository].
final class DriftCheckInRepository implements CheckInRepository {
  /// Creates a repository using [database].
  const DriftCheckInRepository(this._database);

  final AppDatabase _database;

  @override
  Future<List<domain.CheckIn>> getAll() async {
    try {
      final rows = await (_database.select(
        _database.checkIns,
      )..orderBy([(table) => OrderingTerm.desc(table.entryDate)])).get();
      return _attachActivities(rows);
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Future<List<domain.CheckIn>> getBetween(DateTime start, DateTime end) async {
    try {
      final startValue = dateOnlyToStorage(start);
      final endValue = dateOnlyToStorage(end);
      final rows =
          await (_database.select(_database.checkIns)
                ..where(
                  (table) =>
                      table.entryDate.isBetweenValues(startValue, endValue),
                )
                ..orderBy([(table) => OrderingTerm.asc(table.entryDate)]))
              .get();
      return _attachActivities(rows);
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Future<domain.CheckIn?> getById(String id) async {
    try {
      final row = await (_database.select(
        _database.checkIns,
      )..where((table) => table.id.equals(id))).getSingleOrNull();
      return row == null ? null : _attachActivity(row);
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Future<domain.CheckIn?> getByDate(DateTime date) async {
    try {
      final row =
          await (_database.select(_database.checkIns)..where(
                (table) => table.entryDate.equals(dateOnlyToStorage(date)),
              ))
              .getSingleOrNull();
      return row == null ? null : _attachActivity(row);
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Stream<domain.CheckIn?> watchByDate(DateTime date) =>
      (_database.select(_database.checkIns)
            ..where((table) => table.entryDate.equals(dateOnlyToStorage(date))))
          .watchSingleOrNull()
          .asyncMap((row) => row == null ? null : _attachActivity(row));

  @override
  Stream<List<domain.CheckIn>> watchAll() =>
      (_database.select(_database.checkIns)
            ..orderBy([(table) => OrderingTerm.desc(table.entryDate)]))
          .watch()
          .asyncMap(_attachActivities);

  @override
  Future<void> create(domain.CheckIn checkIn) async {
    try {
      CheckInValidator.validateOrThrow(checkIn);
      final existing = await getByDate(checkIn.entryDate);
      if (existing != null) {
        throw const DuplicateEntryDateFailure();
      }
      await _database.transaction(() async {
        await _database
            .into(_database.checkIns)
            .insert(_toInsertCompanion(checkIn));
        await _replaceActivityLinks(checkIn.id, checkIn.activityIds);
      });
    } on AppException {
      rethrow;
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Future<void> update(domain.CheckIn checkIn) async {
    try {
      CheckInValidator.validateOrThrow(checkIn);
      final existing = await getById(checkIn.id);
      if (existing == null) {
        throw const DatabaseFailure('Check-in not found');
      }
      final sameDate = DateUtilsX.isSameDate(
        existing.entryDate,
        checkIn.entryDate,
      );
      if (!sameDate) {
        final occupied = await getByDate(checkIn.entryDate);
        if (occupied != null && occupied.id != checkIn.id) {
          throw const DuplicateEntryDateFailure();
        }
      }
      final updated = checkIn.copyWith(
        createdAt: existing.createdAt,
        updatedAt: DateTime.now().toUtc(),
      );
      await _database.transaction(() async {
        await (_database.update(_database.checkIns)
              ..where((table) => table.id.equals(updated.id)))
            .write(_toUpdateCompanion(updated));
        await _replaceActivityLinks(updated.id, updated.activityIds);
      });
    } on AppException {
      rethrow;
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Future<void> delete(String id) async {
    try {
      await (_database.delete(
        _database.checkIns,
      )..where((table) => table.id.equals(id))).go();
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Future<void> deleteAll() async {
    try {
      await _database.delete(_database.checkIns).go();
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  Future<List<domain.CheckIn>> _attachActivities(List<CheckInRow> rows) async =>
      Future.wait(rows.map(_attachActivity));

  Future<domain.CheckIn> _attachActivity(CheckInRow row) async {
    final links =
        await (_database.select(_database.checkInActivities)
              ..where((table) => table.checkInId.equals(row.id))
              ..orderBy([(table) => OrderingTerm.asc(table.activityId)]))
            .get();
    return _toDomain(
      row,
      links.map((link) => link.activityId).toList(growable: false),
    );
  }

  Future<void> _replaceActivityLinks(
    String checkInId,
    List<String> activityIds,
  ) async {
    await (_database.delete(
      _database.checkInActivities,
    )..where((table) => table.checkInId.equals(checkInId))).go();
    final uniqueIds = activityIds.toSet().toList(growable: false);
    await _database.batch((batch) {
      batch.insertAll(_database.checkInActivities, [
        for (final activityId in uniqueIds)
          CheckInActivitiesCompanion.insert(
            checkInId: checkInId,
            activityId: activityId,
          ),
      ]);
    });
  }

  CheckInsCompanion _toInsertCompanion(domain.CheckIn checkIn) =>
      CheckInsCompanion.insert(
        id: checkIn.id,
        entryDate: dateOnlyToStorage(checkIn.entryDate),
        mood: checkIn.mood,
        energy: Value(checkIn.energy),
        anxiety: Value(checkIn.anxiety),
        focus: Value(checkIn.focus),
        sleepHours: Value(checkIn.sleepHours),
        notes: Value(checkIn.notes),
        createdAt: checkIn.createdAt.toUtc(),
        updatedAt: checkIn.updatedAt.toUtc(),
      );

  CheckInsCompanion _toUpdateCompanion(domain.CheckIn checkIn) =>
      CheckInsCompanion(
        entryDate: Value(dateOnlyToStorage(checkIn.entryDate)),
        mood: Value(checkIn.mood),
        energy: Value(checkIn.energy),
        anxiety: Value(checkIn.anxiety),
        focus: Value(checkIn.focus),
        sleepHours: Value(checkIn.sleepHours),
        notes: Value(checkIn.notes),
        createdAt: Value(checkIn.createdAt.toUtc()),
        updatedAt: Value(checkIn.updatedAt.toUtc()),
      );

  domain.CheckIn _toDomain(CheckInRow row, List<String> activityIds) =>
      domain.CheckIn(
        id: row.id,
        entryDate: dateOnlyFromStorage(row.entryDate),
        mood: row.mood,
        energy: row.energy,
        anxiety: row.anxiety,
        focus: row.focus,
        sleepHours: row.sleepHours,
        activityIds: activityIds,
        notes: row.notes,
        createdAt: row.createdAt.toLocal(),
        updatedAt: row.updatedAt.toLocal(),
      );
}

/// Provides the check-in repository.
final checkInRepositoryProvider = Provider<CheckInRepository>((ref) {
  final database = ref.watch(appDatabaseProvider);
  return DriftCheckInRepository(database);
});
