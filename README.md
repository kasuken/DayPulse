# DayPulse

**Track your wellbeing. Understand your patterns.**

DayPulse is a privacy-first mobile application for tracking daily wellbeing. Record your mood, energy, anxiety, focus, sleep, activities, and optional notes in less than 20 seconds, then explore simple trends and statistics over time.

- Works **completely offline** — no account, no cloud, no trackers
- All data stays **on your device**
- One quick check-in per day
- Simple, deterministic insights (no AI, no medical interpretation)

## Features

- **Onboarding** — one-time introduction with privacy explanation, metric selection, and reminder setup
- **Daily check-in** — mood (required), energy, anxiety, focus, sleep, activities, and notes on a single scrolling page
- **History** — reverse-chronological list of entries with detail view, editing, and deletion
- **Insights** — 7/30/90-day averages, streaks, best/lowest mood days, and simple trend charts
- **Settings** — theme (system/light/dark), metric visibility, daily reminders, activity management, delete-all-data

## Tech Stack

| Concern | Choice |
| --- | --- |
| Framework | Flutter (Material 3) |
| State management | Riverpod |
| Navigation | GoRouter (StatefulShellRoute) |
| Persistence | Drift + SQLite |
| Models | Freezed + json_serializable |
| Reminders | flutter_local_notifications |
| Charts | fl_chart |
| Localization | Flutter gen-l10n (ARB) |

## Architecture

Feature-first clean architecture. Each feature owns its `data`, `domain`, `application`, and `presentation` layers:

```
lib/
  app/            # bootstrap, app widget, router, theme tokens
  core/           # database, errors, notifications, utilities, shared widgets
  features/
    onboarding/
    check_in/
    history/
    insights/
    settings/
  l10n/           # ARB sources + generated localizations
```

Rules: widgets never touch Drift directly, repositories hide persistence details, business logic lives in controllers/services, domain models are immutable.

## Getting Started

```bash
flutter pub get
flutter gen-l10n
dart run build_runner build --delete-conflicting-outputs
flutter run
```

### Quality checks

```bash
flutter analyze
flutter test
```

### Release builds

```bash
flutter build apk
flutter build appbundle
flutter build ios --no-codesign
```

## Privacy

DayPulse stores all data locally in an on-device SQLite database. It sends no analytics, contains no advertising or tracking SDKs, requires no account, and never uploads notes or metrics. A complete delete-all-data flow is available in Settings.

DayPulse provides awareness, not medical conclusions. It is not a medical application, diagnostic tool, or substitute for professional support.
