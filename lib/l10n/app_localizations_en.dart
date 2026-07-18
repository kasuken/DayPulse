// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appTitle => 'DayPulse';

  @override
  String get todayTabLabel => 'Today';

  @override
  String get historyTabLabel => 'History';

  @override
  String get insightsTabLabel => 'Insights';

  @override
  String get settingsTabLabel => 'Settings';

  @override
  String get comingSoon => 'Coming soon';

  @override
  String get entryDetailsTitle => 'Entry details';

  @override
  String get appearanceSettingsTitle => 'Appearance';

  @override
  String get reminderSettingsTitle => 'Reminders';

  @override
  String get privacySettingsTitle => 'Privacy';

  @override
  String get activityManagementTitle => 'Activities';

  @override
  String get onboardingBack => 'Back';

  @override
  String get onboardingContinue => 'Continue';

  @override
  String onboardingProgress(int step, int total) {
    return 'Step $step of $total';
  }

  @override
  String get onboardingWelcomeTitle => 'Welcome to DayPulse';

  @override
  String get onboardingWelcomeTagline =>
      'Track your daily wellbeing in seconds and understand how it changes over time.';

  @override
  String get onboardingPrivacyTitle => 'Private by design';

  @override
  String get onboardingPrivacyLocal =>
      'Your data is stored locally on this device.';

  @override
  String get onboardingPrivacyNoAccount => 'No account is required.';

  @override
  String get onboardingPrivacyNoUpload => 'No wellbeing data is uploaded.';

  @override
  String get onboardingPrivacyDelete => 'You can delete your data anytime.';

  @override
  String get onboardingMetricsTitle => 'Choose your daily metrics';

  @override
  String get onboardingMoodAlwaysOn =>
      'Mood is always included so every entry has a simple daily signal.';

  @override
  String get metricMood => 'Mood';

  @override
  String get metricEnergy => 'Energy';

  @override
  String get metricAnxiety => 'Anxiety';

  @override
  String get metricFocus => 'Focus';

  @override
  String get metricSleep => 'Sleep';

  @override
  String get onboardingReminderTitle => 'Set a gentle reminder';

  @override
  String get onboardingReminderDescription =>
      'Choose whether DayPulse should remind you once a day.';

  @override
  String get reminderSwitchLabel => 'Daily reminder';

  @override
  String get reminderTimeLabel => 'Reminder time';

  @override
  String get changeReminderTime => 'Change reminder time';

  @override
  String get notificationPermissionDenied =>
      'Notifications are off for now. You can still use DayPulse and turn reminders on later in settings.';

  @override
  String get onboardingFinishTitle => 'You\'re ready';

  @override
  String get onboardingFinishDescription =>
      'Your choices are saved on this device. Start with today\'s check-in whenever you\'re ready.';

  @override
  String get startTracking => 'Start tracking';

  @override
  String get dailyReminderTitle => 'Daily check-in';

  @override
  String get dailyReminderBody =>
      'Take a moment to record how your day is going.';

  @override
  String get todayTitle => 'Today';

  @override
  String todayFullDate(String date) {
    return '$date';
  }

  @override
  String get todayPrompt => 'How are you feeling today?';

  @override
  String get startCheckIn => 'Start check-in';

  @override
  String get editCheckIn => 'Edit check-in';

  @override
  String get viewDetails => 'View details';

  @override
  String get notesPreviewLabel => 'Notes';

  @override
  String get activitiesLabel => 'Activities';

  @override
  String get checkInTitleCreate => 'Check-in';

  @override
  String get checkInTitleEdit => 'Edit check-in';

  @override
  String get entryDateLabel => 'Entry date';

  @override
  String get changeDate => 'Change date';

  @override
  String get moodVeryLow => 'Very low';

  @override
  String get moodLow => 'Low';

  @override
  String get moodNeutral => 'Neutral';

  @override
  String get moodGood => 'Good';

  @override
  String get moodGreat => 'Great';

  @override
  String moodValueLabel(int value, String label) {
    return 'Mood $value: $label';
  }

  @override
  String metricValueLabel(String metric, String value) {
    return '$metric: $value';
  }

  @override
  String get metricLow => 'Low';

  @override
  String get metricModerate => 'Moderate';

  @override
  String get metricHigh => 'High';

  @override
  String get anxietyLowerPreferable => 'Lower is preferable.';

  @override
  String sleepHoursValue(String hours) {
    return '$hours h';
  }

  @override
  String get addActivity => 'Add activity';

  @override
  String get newActivityTitle => 'Add activity';

  @override
  String get activityNameLabel => 'Activity name';

  @override
  String get cancel => 'Cancel';

  @override
  String get add => 'Add';

  @override
  String get notesLabel => 'Notes';

  @override
  String get notesHint => 'Anything you\'d like to remember?';

  @override
  String get saveCheckIn => 'Save check-in';

  @override
  String get savingCheckIn => 'Saving…';

  @override
  String get checkInInvalidMessage =>
      'Please check the highlighted values and try again.';

  @override
  String get duplicateEntryDateMessage =>
      'A check-in already exists for that date. Choose another date or edit the existing entry.';

  @override
  String get saveCheckInFailureMessage =>
      'We could not save your check-in. Your changes are still on this screen. Please try again.';

  @override
  String get loadCheckInFailureMessage =>
      'We could not load this check-in. Please try again.';

  @override
  String get checkInNotFoundMessage => 'This check-in could not be found.';

  @override
  String get retry => 'Retry';

  @override
  String get historyEmptyMessage => 'Your check-ins will appear here.';

  @override
  String get createFirstCheckIn => 'Create first check-in';

  @override
  String historyMoodSigned(String value) {
    return 'Mood $value';
  }

  @override
  String metricShortValue(String metric, String value) {
    return '$metric $value';
  }

  @override
  String get createdAtLabel => 'Created';

  @override
  String get updatedAtLabel => 'Last updated';

  @override
  String get delete => 'Delete';

  @override
  String get deleteCheckInTitle => 'Delete this check-in?';

  @override
  String get deleteCheckInMessage => 'This action cannot be undone.';

  @override
  String get noActivities => 'No activities';

  @override
  String get noNotes => 'No notes';

  @override
  String get insightsPeriod7Days => '7 days';

  @override
  String get insightsPeriod30Days => '30 days';

  @override
  String get insightsPeriod90Days => '90 days';

  @override
  String averageMetricTitle(String metric) {
    return 'Average $metric';
  }

  @override
  String get entriesThisMonth => 'Entries this month';

  @override
  String get currentStreak => 'Current streak';

  @override
  String get longestStreak => 'Longest streak';

  @override
  String get bestMoodDay => 'Best mood day';

  @override
  String get lowestMoodDay => 'Lowest mood day';

  @override
  String daysCount(int count) {
    String _temp0 = intl.Intl.pluralLogic(
      count,
      locale: localeName,
      other: '$count days',
      one: '1 day',
      zero: '0 days',
    );
    return '$_temp0';
  }

  @override
  String entriesCount(int count) {
    String _temp0 = intl.Intl.pluralLogic(
      count,
      locale: localeName,
      other: '$count entries',
      one: '1 entry',
      zero: '0 entries',
    );
    return '$_temp0';
  }

  @override
  String trendChartTitle(String metric) {
    return '$metric trend';
  }

  @override
  String trendChartSemantics(String metric) {
    return 'Line chart showing $metric over the selected period.';
  }

  @override
  String get insufficientTrendData =>
      'Add at least two check-ins with this metric to see a trend.';

  @override
  String get settingsMetricsTitle => 'Metrics';

  @override
  String get settingsMoodAlwaysOn => 'Mood is always on.';

  @override
  String get settingsAboutTitle => 'About';

  @override
  String get appearanceSystem => 'System';

  @override
  String get appearanceLight => 'Light';

  @override
  String get appearanceDark => 'Dark';

  @override
  String get notificationPermissionSettingsHint =>
      'Notifications are disabled. Enable them in system settings to use daily reminders.';

  @override
  String get activityDefaultLocked => 'Built-in activity';

  @override
  String get rename => 'Rename';

  @override
  String get renameActivityTitle => 'Rename activity';

  @override
  String get disableActivityTitle => 'Disable activity?';

  @override
  String get disableActivityMessage =>
      'Past check-ins keep this activity, but it will no longer appear for new entries.';

  @override
  String get disable => 'Disable';

  @override
  String get privacyLocalStorageSummary =>
      'DayPulse stores your check-ins, activities, reminders, and preferences locally on this device. No account is required and wellbeing data is not uploaded.';

  @override
  String get exportData => 'Export data';

  @override
  String get planned => 'Planned';

  @override
  String get exportDataPlannedSubtitle =>
      'CSV export is planned for a future update.';

  @override
  String get deleteAllData => 'Delete all data';

  @override
  String get deleteAllDataTitle => 'Delete all data?';

  @override
  String get deleteAllDataMessage =>
      'Delete all DayPulse data from this device? This removes every check-in, activity, and preference. This action cannot be undone.';

  @override
  String aboutVersion(String version, String buildNumber) {
    return 'Version $version ($buildNumber)';
  }

  @override
  String get privacyBlurb =>
      'Privacy-first wellbeing tracking for your device.';

  @override
  String get openSourceLicences => 'Open-source licences';

  @override
  String get aboutDialogTitle => 'About DayPulse';

  @override
  String get close => 'Close';

  @override
  String get notAvailable => 'Not available';

  @override
  String sleepHoursShort(String hours) {
    return '$hours h';
  }
}
