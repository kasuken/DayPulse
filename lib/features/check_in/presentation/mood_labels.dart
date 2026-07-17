import '../../../l10n/app_localizations.dart';

/// Returns the emoji associated with a mood value.
String moodEmoji(int mood) {
  if (mood <= -4) return '😞';
  if (mood <= -2) return '🙁';
  if (mood <= 1) return '😐';
  if (mood <= 3) return '🙂';
  return '😄';
}

/// Returns the localized text label associated with a mood value.
String moodLabel(AppLocalizations l10n, int mood) {
  if (mood <= -4) return l10n.moodVeryLow;
  if (mood <= -2) return l10n.moodLow;
  if (mood <= 1) return l10n.moodNeutral;
  if (mood <= 3) return l10n.moodGood;
  return l10n.moodGreat;
}
