import 'package:daypulse/core/database/app_database.dart';
import 'package:daypulse/core/errors/app_exception.dart';
import 'package:daypulse/features/check_in/data/drift_activity_repository.dart';
import 'package:daypulse/features/check_in/data/drift_check_in_repository.dart';
import 'package:daypulse/features/check_in/domain/activity.dart';
import 'package:daypulse/features/check_in/domain/check_in.dart';
import 'package:drift/native.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  late AppDatabase database;
  late DriftCheckInRepository checkIns;
  late DriftActivityRepository activities;

  setUp(() {
    database = AppDatabase(NativeDatabase.memory());
    checkIns = DriftCheckInRepository(database);
    activities = DriftActivityRepository(database);
  });

  tearDown(() async {
    await database.close();
  });

  group('Drift repositories', () {
    test('seeds default activities', () async {
      final active = await activities.getActive();

      expect(active, hasLength(16));
      expect(
        active.first,
        const Activity(
          id: 'default-work',
          name: 'Work',
          isDefault: true,
          isActive: true,
          sortOrder: 0,
        ),
      );
    });

    test(
      'creates, reads, updates, and deletes check-ins with activities',
      () async {
        final created = _checkIn(
          id: 'one',
          day: 10,
          mood: 1,
          activityIds: const ['default-work', 'default-reading'],
        );
        await checkIns.create(created);

        final byDate = await checkIns.getByDate(DateTime(2024, 1, 10, 12));
        expect(byDate?.activityIds, ['default-reading', 'default-work']);
        expect(byDate?.entryDate, DateTime(2024, 1, 10));

        final updated = created.copyWith(
          mood: 3,
          activityIds: const ['default-work'],
          entryDate: DateTime(2024, 1, 11),
        );
        await checkIns.update(updated);

        expect(await checkIns.getByDate(DateTime(2024, 1, 10)), isNull);
        final moved = await checkIns.getById('one');
        expect(moved?.mood, 3);
        expect(moved?.createdAt, created.createdAt.toLocal());
        expect(moved?.updatedAt.isAfter(created.updatedAt.toLocal()), isTrue);

        await checkIns.delete('one');
        expect(await checkIns.getAll(), isEmpty);
      },
    );

    test('filters by inclusive date range', () async {
      await checkIns.create(_checkIn(id: 'one', day: 1, mood: 1));
      await checkIns.create(_checkIn(id: 'two', day: 2, mood: 2));
      await checkIns.create(_checkIn(id: 'three', day: 3, mood: 3));

      final result = await checkIns.getBetween(
        DateTime(2024, 1, 2),
        DateTime(2024, 1, 3),
      );

      expect(result.map((entry) => entry.id), ['two', 'three']);
    });

    test(
      'throws DuplicateEntryDateFailure on create and occupied date update',
      () async {
        await checkIns.create(_checkIn(id: 'one', day: 1, mood: 1));
        await checkIns.create(_checkIn(id: 'two', day: 2, mood: 2));

        expect(
          () => checkIns.create(_checkIn(id: 'duplicate', day: 1, mood: 3)),
          throwsA(isA<DuplicateEntryDateFailure>()),
        );
        expect(
          () => checkIns.update(_checkIn(id: 'two', day: 1, mood: 2)),
          throwsA(isA<DuplicateEntryDateFailure>()),
        );
      },
    );

    test('deactivation keeps historical check-in references', () async {
      await activities.add(
        const Activity(
          id: 'custom',
          name: 'Custom',
          isDefault: false,
          isActive: true,
          sortOrder: 100,
        ),
      );
      await checkIns.create(
        _checkIn(id: 'one', day: 1, mood: 1, activityIds: const ['custom']),
      );

      await activities.deactivate('custom');

      final active = await activities.getActive();
      expect(active.map((activity) => activity.id), isNot(contains('custom')));
      expect((await checkIns.getById('one'))?.activityIds, ['custom']);
    });

    test('deleteAll removes check-ins and cascades activity links', () async {
      await checkIns.create(
        _checkIn(
          id: 'one',
          day: 1,
          mood: 1,
          activityIds: const ['default-work'],
        ),
      );

      await checkIns.deleteAll();

      expect(await checkIns.getAll(), isEmpty);
      expect(await activities.getActive(), isNotEmpty);
    });
  });
}

CheckIn _checkIn({
  required String id,
  required int day,
  required int mood,
  List<String> activityIds = const [],
}) {
  final date = DateTime(2024, 1, day);
  return CheckIn(
    id: id,
    entryDate: date,
    mood: mood,
    activityIds: activityIds,
    createdAt: DateTime.utc(2024, 1, day),
    updatedAt: DateTime.utc(2024, 1, day),
  );
}
