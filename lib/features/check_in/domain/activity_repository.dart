import 'activity.dart';

/// Persistence contract for activity tags.
abstract interface class ActivityRepository {
  /// Returns active activities ordered by sort order then name.
  Future<List<Activity>> getActive();

  /// Watches active activities ordered by sort order then name.
  Stream<List<Activity>> watchActive();

  /// Adds [activity].
  Future<void> add(Activity activity);

  /// Updates [activity].
  Future<void> update(Activity activity);

  /// Soft-deletes an activity by making it inactive.
  Future<void> deactivate(String id);

  /// Ensures default activities exist.
  Future<void> seedDefaults();

  /// Removes custom activities and restores built-in defaults.
  Future<void> resetToDefaults();
}
