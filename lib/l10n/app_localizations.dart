import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_en.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of AppLocalizations
/// returned by `AppLocalizations.of(context)`.
///
/// Applications need to include `AppLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'l10n/app_localizations.dart';
///
/// return MaterialApp(
///   localizationsDelegates: AppLocalizations.localizationsDelegates,
///   supportedLocales: AppLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the AppLocalizations.supportedLocales
/// property.
abstract class AppLocalizations {
  AppLocalizations(String locale)
    : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static AppLocalizations? of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations);
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
        delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
      ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[Locale('en')];

  /// The application name shown in the app switcher.
  ///
  /// In en, this message translates to:
  /// **'DayPulse'**
  String get appTitle;

  /// Bottom navigation label for today's check-in.
  ///
  /// In en, this message translates to:
  /// **'Today'**
  String get todayTabLabel;

  /// Bottom navigation label for history.
  ///
  /// In en, this message translates to:
  /// **'History'**
  String get historyTabLabel;

  /// Bottom navigation label for insights.
  ///
  /// In en, this message translates to:
  /// **'Insights'**
  String get insightsTabLabel;

  /// Bottom navigation label for settings.
  ///
  /// In en, this message translates to:
  /// **'Settings'**
  String get settingsTabLabel;

  /// Placeholder text for screens implemented later.
  ///
  /// In en, this message translates to:
  /// **'Coming soon'**
  String get comingSoon;

  /// Title for a check-in detail screen.
  ///
  /// In en, this message translates to:
  /// **'Entry details'**
  String get entryDetailsTitle;

  /// Title for appearance settings.
  ///
  /// In en, this message translates to:
  /// **'Appearance'**
  String get appearanceSettingsTitle;

  /// Title for reminder settings.
  ///
  /// In en, this message translates to:
  /// **'Reminders'**
  String get reminderSettingsTitle;

  /// Title for privacy settings.
  ///
  /// In en, this message translates to:
  /// **'Privacy'**
  String get privacySettingsTitle;

  /// Title for activity management settings.
  ///
  /// In en, this message translates to:
  /// **'Activities'**
  String get activityManagementTitle;

  /// Button label to go back in onboarding.
  ///
  /// In en, this message translates to:
  /// **'Back'**
  String get onboardingBack;

  /// Button label to continue onboarding.
  ///
  /// In en, this message translates to:
  /// **'Continue'**
  String get onboardingContinue;

  /// Accessible onboarding step progress.
  ///
  /// In en, this message translates to:
  /// **'Step {step} of {total}'**
  String onboardingProgress(int step, int total);

  /// Onboarding welcome title.
  ///
  /// In en, this message translates to:
  /// **'Welcome to DayPulse'**
  String get onboardingWelcomeTitle;

  /// Onboarding welcome tagline.
  ///
  /// In en, this message translates to:
  /// **'Track your daily wellbeing in seconds and understand how it changes over time.'**
  String get onboardingWelcomeTagline;

  /// Onboarding privacy step title.
  ///
  /// In en, this message translates to:
  /// **'Private by design'**
  String get onboardingPrivacyTitle;

  /// Privacy onboarding bullet about local storage.
  ///
  /// In en, this message translates to:
  /// **'Your data is stored locally on this device.'**
  String get onboardingPrivacyLocal;

  /// Privacy onboarding bullet about accounts.
  ///
  /// In en, this message translates to:
  /// **'No account is required.'**
  String get onboardingPrivacyNoAccount;

  /// Privacy onboarding bullet about uploads.
  ///
  /// In en, this message translates to:
  /// **'No wellbeing data is uploaded.'**
  String get onboardingPrivacyNoUpload;

  /// Privacy onboarding bullet about deletion.
  ///
  /// In en, this message translates to:
  /// **'You can delete your data anytime.'**
  String get onboardingPrivacyDelete;

  /// Onboarding metrics step title.
  ///
  /// In en, this message translates to:
  /// **'Choose your daily metrics'**
  String get onboardingMetricsTitle;

  /// Explanation that mood cannot be disabled.
  ///
  /// In en, this message translates to:
  /// **'Mood is always included so every entry has a simple daily signal.'**
  String get onboardingMoodAlwaysOn;

  /// Metric label for mood.
  ///
  /// In en, this message translates to:
  /// **'Mood'**
  String get metricMood;

  /// Metric label for energy.
  ///
  /// In en, this message translates to:
  /// **'Energy'**
  String get metricEnergy;

  /// Metric label for anxiety.
  ///
  /// In en, this message translates to:
  /// **'Anxiety'**
  String get metricAnxiety;

  /// Metric label for focus.
  ///
  /// In en, this message translates to:
  /// **'Focus'**
  String get metricFocus;

  /// Metric label for sleep.
  ///
  /// In en, this message translates to:
  /// **'Sleep'**
  String get metricSleep;

  /// Onboarding reminder step title.
  ///
  /// In en, this message translates to:
  /// **'Set a gentle reminder'**
  String get onboardingReminderTitle;

  /// Onboarding reminder explanation.
  ///
  /// In en, this message translates to:
  /// **'Choose whether DayPulse should remind you once a day.'**
  String get onboardingReminderDescription;

  /// Switch label for daily reminder.
  ///
  /// In en, this message translates to:
  /// **'Daily reminder'**
  String get reminderSwitchLabel;

  /// Label for reminder time picker.
  ///
  /// In en, this message translates to:
  /// **'Reminder time'**
  String get reminderTimeLabel;

  /// Button label for changing reminder time.
  ///
  /// In en, this message translates to:
  /// **'Change reminder time'**
  String get changeReminderTime;

  /// Message shown after notification permission is denied.
  ///
  /// In en, this message translates to:
  /// **'Notifications are off for now. You can still use DayPulse and turn reminders on later in settings.'**
  String get notificationPermissionDenied;

  /// Onboarding finish title.
  ///
  /// In en, this message translates to:
  /// **'You\'re ready'**
  String get onboardingFinishTitle;

  /// Onboarding finish description.
  ///
  /// In en, this message translates to:
  /// **'Your choices are saved on this device. Start with today\'s check-in whenever you\'re ready.'**
  String get onboardingFinishDescription;

  /// Primary button at the end of onboarding.
  ///
  /// In en, this message translates to:
  /// **'Start tracking'**
  String get startTracking;

  /// Local notification title for daily reminders.
  ///
  /// In en, this message translates to:
  /// **'Daily check-in'**
  String get dailyReminderTitle;

  /// Local notification body for daily reminders.
  ///
  /// In en, this message translates to:
  /// **'Take a moment to record how your day is going.'**
  String get dailyReminderBody;

  /// Today screen title.
  ///
  /// In en, this message translates to:
  /// **'Today'**
  String get todayTitle;

  /// Formatted full date on the Today screen.
  ///
  /// In en, this message translates to:
  /// **'{date}'**
  String todayFullDate(String date);

  /// Prompt shown when no check-in exists today.
  ///
  /// In en, this message translates to:
  /// **'How are you feeling today?'**
  String get todayPrompt;

  /// Button to start a check-in.
  ///
  /// In en, this message translates to:
  /// **'Start check-in'**
  String get startCheckIn;

  /// Button to edit an existing check-in.
  ///
  /// In en, this message translates to:
  /// **'Edit check-in'**
  String get editCheckIn;

  /// Button to view check-in details.
  ///
  /// In en, this message translates to:
  /// **'View details'**
  String get viewDetails;

  /// Label for notes preview.
  ///
  /// In en, this message translates to:
  /// **'Notes'**
  String get notesPreviewLabel;

  /// Label for selected activities.
  ///
  /// In en, this message translates to:
  /// **'Activities'**
  String get activitiesLabel;

  /// Title for creating a check-in.
  ///
  /// In en, this message translates to:
  /// **'Check-in'**
  String get checkInTitleCreate;

  /// Title for editing a check-in.
  ///
  /// In en, this message translates to:
  /// **'Edit check-in'**
  String get checkInTitleEdit;

  /// Label for check-in entry date.
  ///
  /// In en, this message translates to:
  /// **'Entry date'**
  String get entryDateLabel;

  /// Button label to change the entry date.
  ///
  /// In en, this message translates to:
  /// **'Change date'**
  String get changeDate;

  /// Mood label for very low values.
  ///
  /// In en, this message translates to:
  /// **'Very low'**
  String get moodVeryLow;

  /// Mood label for low values.
  ///
  /// In en, this message translates to:
  /// **'Low'**
  String get moodLow;

  /// Mood label for neutral values.
  ///
  /// In en, this message translates to:
  /// **'Neutral'**
  String get moodNeutral;

  /// Mood label for good values.
  ///
  /// In en, this message translates to:
  /// **'Good'**
  String get moodGood;

  /// Mood label for great values.
  ///
  /// In en, this message translates to:
  /// **'Great'**
  String get moodGreat;

  /// Accessible mood value label.
  ///
  /// In en, this message translates to:
  /// **'Mood {value}: {label}'**
  String moodValueLabel(int value, String label);

  /// Accessible metric value label.
  ///
  /// In en, this message translates to:
  /// **'{metric}: {value}'**
  String metricValueLabel(String metric, String value);

  /// Low label under metric sliders.
  ///
  /// In en, this message translates to:
  /// **'Low'**
  String get metricLow;

  /// Moderate label under metric sliders.
  ///
  /// In en, this message translates to:
  /// **'Moderate'**
  String get metricModerate;

  /// High label under metric sliders.
  ///
  /// In en, this message translates to:
  /// **'High'**
  String get metricHigh;

  /// Low anxiety range label under the anxiety slider.
  ///
  /// In en, this message translates to:
  /// **'Calm'**
  String get anxietyRangeCalm;

  /// Moderate anxiety range label under the anxiety slider.
  ///
  /// In en, this message translates to:
  /// **'Moderate'**
  String get anxietyRangeModerate;

  /// High anxiety range label under the anxiety slider.
  ///
  /// In en, this message translates to:
  /// **'Very anxious'**
  String get anxietyRangeVeryAnxious;

  /// Helper text for anxiety slider.
  ///
  /// In en, this message translates to:
  /// **'Lower is preferable.'**
  String get anxietyLowerPreferable;

  /// Sleep duration value in hours.
  ///
  /// In en, this message translates to:
  /// **'{hours} h'**
  String sleepHoursValue(String hours);

  /// Action chip label to add an activity.
  ///
  /// In en, this message translates to:
  /// **'Add activity'**
  String get addActivity;

  /// Dialog title for adding an activity.
  ///
  /// In en, this message translates to:
  /// **'Add activity'**
  String get newActivityTitle;

  /// Text field label for a custom activity name.
  ///
  /// In en, this message translates to:
  /// **'Activity name'**
  String get activityNameLabel;

  /// Generic cancel button.
  ///
  /// In en, this message translates to:
  /// **'Cancel'**
  String get cancel;

  /// Title for the confirmation dialog shown when leaving a check-in with unsaved edits.
  ///
  /// In en, this message translates to:
  /// **'Discard changes?'**
  String get discardChangesTitle;

  /// Message for the confirmation dialog shown when leaving a check-in with unsaved edits.
  ///
  /// In en, this message translates to:
  /// **'Your edits to this check-in will be lost.'**
  String get discardChangesMessage;

  /// Button label that confirms discarding unsaved check-in edits.
  ///
  /// In en, this message translates to:
  /// **'Discard'**
  String get discardChangesConfirm;

  /// Generic add button.
  ///
  /// In en, this message translates to:
  /// **'Add'**
  String get add;

  /// Text field label for notes.
  ///
  /// In en, this message translates to:
  /// **'Notes'**
  String get notesLabel;

  /// Text field hint for notes.
  ///
  /// In en, this message translates to:
  /// **'Anything you\'d like to remember?'**
  String get notesHint;

  /// Button label to save check-in.
  ///
  /// In en, this message translates to:
  /// **'Save check-in'**
  String get saveCheckIn;

  /// Label shown while saving check-in.
  ///
  /// In en, this message translates to:
  /// **'Saving…'**
  String get savingCheckIn;

  /// Validation error for invalid check-in values.
  ///
  /// In en, this message translates to:
  /// **'Please check the highlighted values and try again.'**
  String get checkInInvalidMessage;

  /// Error shown when saving to a date that already has a different entry.
  ///
  /// In en, this message translates to:
  /// **'A check-in already exists for that date. Choose another date or edit the existing entry.'**
  String get duplicateEntryDateMessage;

  /// Friendly save failure message.
  ///
  /// In en, this message translates to:
  /// **'We could not save your check-in. Your changes are still on this screen. Please try again.'**
  String get saveCheckInFailureMessage;

  /// Friendly load failure message.
  ///
  /// In en, this message translates to:
  /// **'We could not load this check-in. Please try again.'**
  String get loadCheckInFailureMessage;

  /// Message shown when an edit check-in is missing.
  ///
  /// In en, this message translates to:
  /// **'This check-in could not be found.'**
  String get checkInNotFoundMessage;

  /// Retry button label.
  ///
  /// In en, this message translates to:
  /// **'Retry'**
  String get retry;

  /// Empty state message for the history screen.
  ///
  /// In en, this message translates to:
  /// **'Your check-ins will appear here.'**
  String get historyEmptyMessage;

  /// Button to create the first check-in from history.
  ///
  /// In en, this message translates to:
  /// **'Create first check-in'**
  String get createFirstCheckIn;

  /// History row mood value with sign.
  ///
  /// In en, this message translates to:
  /// **'Mood {value}'**
  String historyMoodSigned(String value);

  /// Compact metric and value label.
  ///
  /// In en, this message translates to:
  /// **'{metric} {value}'**
  String metricShortValue(String metric, String value);

  /// Label for a created timestamp.
  ///
  /// In en, this message translates to:
  /// **'Created'**
  String get createdAtLabel;

  /// Label for an updated timestamp.
  ///
  /// In en, this message translates to:
  /// **'Last updated'**
  String get updatedAtLabel;

  /// Generic delete button.
  ///
  /// In en, this message translates to:
  /// **'Delete'**
  String get delete;

  /// Check-in deletion confirmation title.
  ///
  /// In en, this message translates to:
  /// **'Delete this check-in?'**
  String get deleteCheckInTitle;

  /// Check-in deletion confirmation body.
  ///
  /// In en, this message translates to:
  /// **'This action cannot be undone.'**
  String get deleteCheckInMessage;

  /// Message when an entry has no visible activities.
  ///
  /// In en, this message translates to:
  /// **'No activities'**
  String get noActivities;

  /// Message when an entry has no notes.
  ///
  /// In en, this message translates to:
  /// **'No notes'**
  String get noNotes;

  /// Insights period filter label for seven days.
  ///
  /// In en, this message translates to:
  /// **'7 days'**
  String get insightsPeriod7Days;

  /// Insights period filter label for thirty days.
  ///
  /// In en, this message translates to:
  /// **'30 days'**
  String get insightsPeriod30Days;

  /// Insights period filter label for ninety days.
  ///
  /// In en, this message translates to:
  /// **'90 days'**
  String get insightsPeriod90Days;

  /// Title for an average metric summary card.
  ///
  /// In en, this message translates to:
  /// **'Average {metric}'**
  String averageMetricTitle(String metric);

  /// Summary card title for entries in the current month.
  ///
  /// In en, this message translates to:
  /// **'Entries this month'**
  String get entriesThisMonth;

  /// Summary card title for current streak.
  ///
  /// In en, this message translates to:
  /// **'Current streak'**
  String get currentStreak;

  /// Summary card title for longest streak.
  ///
  /// In en, this message translates to:
  /// **'Longest streak'**
  String get longestStreak;

  /// Summary card title for best mood day.
  ///
  /// In en, this message translates to:
  /// **'Best mood day'**
  String get bestMoodDay;

  /// Summary card title for lowest mood day.
  ///
  /// In en, this message translates to:
  /// **'Lowest mood day'**
  String get lowestMoodDay;

  /// Number of days.
  ///
  /// In en, this message translates to:
  /// **'{count, plural, =0{0 days} =1{1 day} other{{count} days}}'**
  String daysCount(int count);

  /// Number of check-in entries.
  ///
  /// In en, this message translates to:
  /// **'{count, plural, =0{0 entries} =1{1 entry} other{{count} entries}}'**
  String entriesCount(int count);

  /// Trend chart title for a metric.
  ///
  /// In en, this message translates to:
  /// **'{metric} trend'**
  String trendChartTitle(String metric);

  /// Accessibility label for trend charts.
  ///
  /// In en, this message translates to:
  /// **'Line chart showing {metric} over the selected period.'**
  String trendChartSemantics(String metric);

  /// Empty state for trend charts with fewer than two points.
  ///
  /// In en, this message translates to:
  /// **'Add at least two check-ins with this metric to see a trend.'**
  String get insufficientTrendData;

  /// Settings section title for metric switches.
  ///
  /// In en, this message translates to:
  /// **'Metrics'**
  String get settingsMetricsTitle;

  /// Settings helper text that mood cannot be disabled.
  ///
  /// In en, this message translates to:
  /// **'Mood is always on.'**
  String get settingsMoodAlwaysOn;

  /// Settings section title for app information.
  ///
  /// In en, this message translates to:
  /// **'About'**
  String get settingsAboutTitle;

  /// Theme setting option for system theme.
  ///
  /// In en, this message translates to:
  /// **'System'**
  String get appearanceSystem;

  /// Theme setting option for light theme.
  ///
  /// In en, this message translates to:
  /// **'Light'**
  String get appearanceLight;

  /// Theme setting option for dark theme.
  ///
  /// In en, this message translates to:
  /// **'Dark'**
  String get appearanceDark;

  /// Reminder settings message when permission is denied.
  ///
  /// In en, this message translates to:
  /// **'Notifications are disabled. Enable them in system settings to use daily reminders.'**
  String get notificationPermissionSettingsHint;

  /// Reminder settings message shown when scheduling the daily notification fails.
  ///
  /// In en, this message translates to:
  /// **'Couldn\'t schedule the reminder. Please try again.'**
  String get reminderScheduleFailed;

  /// Subtitle for a locked default activity.
  ///
  /// In en, this message translates to:
  /// **'Built-in activity'**
  String get activityDefaultLocked;

  /// Generic rename action.
  ///
  /// In en, this message translates to:
  /// **'Rename'**
  String get rename;

  /// Dialog title for renaming an activity.
  ///
  /// In en, this message translates to:
  /// **'Rename activity'**
  String get renameActivityTitle;

  /// Dialog title for disabling an activity.
  ///
  /// In en, this message translates to:
  /// **'Disable activity?'**
  String get disableActivityTitle;

  /// Confirmation message for activity soft deletion.
  ///
  /// In en, this message translates to:
  /// **'Past check-ins keep this activity, but it will no longer appear for new entries.'**
  String get disableActivityMessage;

  /// Generic disable action.
  ///
  /// In en, this message translates to:
  /// **'Disable'**
  String get disable;

  /// Privacy summary explaining local-only storage.
  ///
  /// In en, this message translates to:
  /// **'DayPulse stores your check-ins, activities, reminders, and preferences locally on this device. No account is required and wellbeing data is not uploaded.'**
  String get privacyLocalStorageSummary;

  /// Privacy action label for data export.
  ///
  /// In en, this message translates to:
  /// **'Export data'**
  String get exportData;

  /// Badge for planned functionality.
  ///
  /// In en, this message translates to:
  /// **'Planned'**
  String get planned;

  /// Subtitle for disabled export tile.
  ///
  /// In en, this message translates to:
  /// **'CSV export is planned for a future update.'**
  String get exportDataPlannedSubtitle;

  /// Privacy action label for deleting all data.
  ///
  /// In en, this message translates to:
  /// **'Delete all data'**
  String get deleteAllData;

  /// Title for delete all data confirmation.
  ///
  /// In en, this message translates to:
  /// **'Delete all data?'**
  String get deleteAllDataTitle;

  /// Exact confirmation message for deleting all data.
  ///
  /// In en, this message translates to:
  /// **'Delete all DayPulse data from this device? This removes every check-in, activity, and preference. This action cannot be undone.'**
  String get deleteAllDataMessage;

  /// About version and build number.
  ///
  /// In en, this message translates to:
  /// **'Version {version} ({buildNumber})'**
  String aboutVersion(String version, String buildNumber);

  /// Short about privacy blurb.
  ///
  /// In en, this message translates to:
  /// **'Privacy-first wellbeing tracking for your device.'**
  String get privacyBlurb;

  /// Tile label for showing open-source licences.
  ///
  /// In en, this message translates to:
  /// **'Open-source licences'**
  String get openSourceLicences;

  /// About dialog title.
  ///
  /// In en, this message translates to:
  /// **'About DayPulse'**
  String get aboutDialogTitle;

  /// Generic close button.
  ///
  /// In en, this message translates to:
  /// **'Close'**
  String get close;

  /// Generic unavailable value label.
  ///
  /// In en, this message translates to:
  /// **'Not available'**
  String get notAvailable;

  /// Short sleep duration value in hours.
  ///
  /// In en, this message translates to:
  /// **'{hours} h'**
  String sleepHoursShort(String hours);
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  Future<AppLocalizations> load(Locale locale) {
    return SynchronousFuture<AppLocalizations>(lookupAppLocalizations(locale));
  }

  @override
  bool isSupported(Locale locale) =>
      <String>['en'].contains(locale.languageCode);

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}

AppLocalizations lookupAppLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'en':
      return AppLocalizationsEn();
  }

  throw FlutterError(
    'AppLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
    'an issue with the localizations generation tool. Please file an issue '
    'on GitHub with a reproducible sample app and the gen-l10n configuration '
    'that was used.',
  );
}
