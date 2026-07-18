import 'package:drift/drift.dart';
import 'package:drift_flutter/drift_flutter.dart';

part 'app_database.g.dart';

/// Converts a local calendar date to stable storage text.
String dateOnlyToStorage(DateTime date) {
  final localDate = DateTime(date.year, date.month, date.day);
  final year = localDate.year.toString().padLeft(4, '0');
  final month = localDate.month.toString().padLeft(2, '0');
  final day = localDate.day.toString().padLeft(2, '0');
  return '$year-$month-$day';
}

/// Parses stable storage text as a local calendar date.
DateTime dateOnlyFromStorage(String value) {
  final year = int.parse(value.substring(0, 4));
  final month = int.parse(value.substring(5, 7));
  final day = int.parse(value.substring(8, 10));
  return DateTime(year, month, day);
}

/// Check-in rows, one per local calendar date.
@DataClassName('CheckInRow')
class CheckIns extends Table {
  @override
  String get tableName => 'check_ins';

  TextColumn get id => text()();
  TextColumn get entryDate => text()();
  IntColumn get mood => integer()();
  IntColumn get energy => integer().nullable()();
  IntColumn get anxiety => integer().nullable()();
  IntColumn get focus => integer().nullable()();
  RealColumn get sleepHours => real().nullable()();
  TextColumn get notes => text().nullable()();
  DateTimeColumn get createdAt => dateTime()();
  DateTimeColumn get updatedAt => dateTime()();

  @override
  Set<Column<Object>> get primaryKey => {id};

  @override
  List<Set<Column<Object>>> get uniqueKeys => [
    {entryDate},
  ];
}

/// Activity tag rows.
@DataClassName('ActivityRow')
class Activities extends Table {
  @override
  String get tableName => 'activities';

  TextColumn get id => text()();
  TextColumn get name => text()();
  BoolColumn get isDefault => boolean()();
  BoolColumn get isActive => boolean()();
  IntColumn get sortOrder => integer()();
  DateTimeColumn get createdAt => dateTime()();
  DateTimeColumn get updatedAt => dateTime()();

  @override
  Set<Column<Object>> get primaryKey => {id};
}

/// Join table between check-ins and activities.
@DataClassName('CheckInActivityRow')
class CheckInActivities extends Table {
  @override
  String get tableName => 'check_in_activities';

  TextColumn get checkInId =>
      text().references(CheckIns, #id, onDelete: KeyAction.cascade)();
  TextColumn get activityId => text().references(Activities, #id)();

  @override
  Set<Column<Object>> get primaryKey => {checkInId, activityId};
}

/// Drift database for DayPulse local persistence.
@DriftDatabase(tables: [CheckIns, Activities, CheckInActivities])
class AppDatabase extends _$AppDatabase {
  /// Creates a database with [executor], or the app database when omitted.
  AppDatabase([QueryExecutor? executor]) : super(executor ?? _openConnection());

  @override
  int get schemaVersion => 1;

  @override
  MigrationStrategy get migration => MigrationStrategy(
    beforeOpen: (details) async {
      await customStatement('PRAGMA foreign_keys = ON');
    },
    onCreate: (migrator) async {
      await migrator.createAll();
      await customStatement(
        'CREATE INDEX IF NOT EXISTS check_ins_entry_date_idx '
        'ON check_ins (entry_date)',
      );
      await seedDefaultActivities();
    },
  );

  /// Inserts built-in default activities if they are not already present.
  Future<void> seedDefaultActivities() async {
    final now = DateTime.now().toUtc();
    await batch((batch) {
      batch.insertAllOnConflictUpdate(activities, defaultActivitySeeds(now));
    });
  }
}

QueryExecutor _openConnection() => driftDatabase(name: 'daypulse');

/// Built-in activity seed rows with stable ids and sort order.
List<ActivitiesCompanion> defaultActivitySeeds(DateTime now) {
  const namesById = <(String, String)>[
    ('default-work', 'Work'),
    ('default-exercise', 'Exercise'),
    ('default-walking', 'Walking'),
    ('default-reading', 'Reading'),
    ('default-gaming', 'Gaming'),
    ('default-family', 'Family'),
    ('default-friends', 'Friends'),
    ('default-travel', 'Travel'),
    ('default-relaxing', 'Relaxing'),
    ('default-housework', 'Housework'),
    ('default-cooking', 'Cooking'),
    ('default-shopping', 'Shopping'),
    ('default-movies', 'Movies'),
    ('default-music', 'Music'),
    ('default-study', 'Study'),
    ('default-meditation', 'Meditation'),
  ];

  return [
    for (final (index, item) in namesById.indexed)
      ActivitiesCompanion.insert(
        id: item.$1,
        name: item.$2,
        isDefault: true,
        isActive: true,
        sortOrder: index,
        createdAt: now,
        updatedAt: now,
      ),
  ];
}
