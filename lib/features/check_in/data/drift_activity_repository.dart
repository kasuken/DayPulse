import 'package:drift/drift.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/database/app_database.dart';
import '../../../core/database/database_provider.dart';
import '../../../core/errors/app_exception.dart';
import '../domain/activity.dart' as domain;
import '../domain/activity_repository.dart';

/// Drift-backed implementation of [ActivityRepository].
final class DriftActivityRepository implements ActivityRepository {
  /// Creates a repository using [database].
  const DriftActivityRepository(this._database);

  final AppDatabase _database;

  @override
  Future<List<domain.Activity>> getActive() async {
    try {
      final rows = await _activeQuery().get();
      return rows.map(_toDomain).toList(growable: false);
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Stream<List<domain.Activity>> watchActive() => _activeQuery().watch().map(
    (rows) => rows.map(_toDomain).toList(growable: false),
  );

  @override
  Future<void> add(domain.Activity activity) async {
    try {
      final now = DateTime.now().toUtc();
      await _database
          .into(_database.activities)
          .insert(
            ActivitiesCompanion.insert(
              id: activity.id,
              name: activity.name,
              isDefault: activity.isDefault,
              isActive: activity.isActive,
              sortOrder: activity.sortOrder,
              createdAt: now,
              updatedAt: now,
            ),
          );
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Future<void> update(domain.Activity activity) async {
    try {
      final existing = await (_database.select(
        _database.activities,
      )..where((table) => table.id.equals(activity.id))).getSingleOrNull();
      if (existing == null) {
        throw const DatabaseFailure('Activity not found');
      }
      await (_database.update(
        _database.activities,
      )..where((table) => table.id.equals(activity.id))).write(
        ActivitiesCompanion(
          name: Value(activity.name),
          isDefault: Value(activity.isDefault),
          isActive: Value(activity.isActive),
          sortOrder: Value(activity.sortOrder),
          updatedAt: Value(DateTime.now().toUtc()),
        ),
      );
    } on AppException {
      rethrow;
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Future<void> deactivate(String id) async {
    try {
      await (_database.update(
        _database.activities,
      )..where((table) => table.id.equals(id))).write(
        ActivitiesCompanion(
          isActive: const Value(false),
          updatedAt: Value(DateTime.now().toUtc()),
        ),
      );
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Future<void> seedDefaults() async {
    try {
      await _database.seedDefaultActivities();
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  @override
  Future<void> resetToDefaults() async {
    try {
      await _database.delete(_database.activities).go();
      await _database.seedDefaultActivities();
    } on Object catch (error) {
      throw DatabaseFailure(error.toString());
    }
  }

  SimpleSelectStatement<$ActivitiesTable, ActivityRow> _activeQuery() =>
      (_database.select(_database.activities)
        ..where((table) => table.isActive.equals(true))
        ..orderBy([
          (table) => OrderingTerm.asc(table.sortOrder),
          (table) => OrderingTerm.asc(table.name),
        ]));

  domain.Activity _toDomain(ActivityRow row) => domain.Activity(
    id: row.id,
    name: row.name,
    isDefault: row.isDefault,
    isActive: row.isActive,
    sortOrder: row.sortOrder,
  );
}

/// Provides the activity repository.
final activityRepositoryProvider = Provider<ActivityRepository>((ref) {
  final database = ref.watch(appDatabaseProvider);
  return DriftActivityRepository(database);
});
