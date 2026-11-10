# Changelog

Notable changes to DayPulse, following [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and semantic versioning.

## [1.2.0] - 2026-11-11

### Added

- **Quick check-in** - Save directly after choosing your mood, without scrolling past optional metrics, activities and notes. The full form remains available on the same page.
- **History search** - Find check-ins by a displayed date or a word in your notes, entirely on your device. Matching counts and a clear-search action make it easy to return to the full journal.
- **Settings shortcuts** - Jump directly to appearance, metrics, reminders, activities, data controls or app information.

### Changed

- **Journal presentation** - History groups entries by month with mood faces and note previews. Insights has a clearer period picker, statistic cards and trend chart, with the detailed table available on demand.
- **Entry detail** - Activity chips, a separate notes section and quieter timestamps make saved check-ins easier to scan.
- **Settings layout** - Consistent panel spacing and compact built-in activity switches reduce scrolling while retaining custom activity editing and reordering.

### Fixed

- **Save feedback** - Failed check-in saves move keyboard focus to the error summary, keeping the draft intact.
- **Custom activity entry** - The add button becomes available as you type, without requiring you to leave the name field first.

### Release limitations

No data migration is needed from v1.1.0. English only; no accounts, cloud synchronization, encrypted storage, import/restore or reliable closed-app reminders. Browser data remains tied to its origin and may be lost when cleared or evicted. A GitHub release does not by itself confirm deployment.

## [1.1.0] - 2026-11-07

### Added

- **Update prompt** - When a new version has been downloaded, a "New version available - Reload" message appears; the app no longer switches versions silently underneath a check-in (issue #58).
- **Permissions-Policy header** - Browser features DayPulse never uses (camera, microphone, geolocation, payment, USB and other device APIs) are disabled on the hosted site (issue #63).

### Changed

- **Journal storage** - Check-ins, activities and settings now live in IndexedDB instead of localStorage. Existing v1.0.0 data is copied across automatically the first time the new version runs, and the old copies are then removed (issue #61).
- **Atomic updates without Web Locks** - Conditional writes are single IndexedDB transactions, so saving no longer depends on the Web Locks API (older iOS versions lack it); the one-time migration also works without it (issue #63).

### Release limitations

Users still on v1.0.0 move to this version only after closing all open DayPulse tabs or windows, because v1.0.0 has no update prompt. Everything else from v1.0.0 still applies: English only, no accounts, cloud synchronization, encrypted storage or import/restore, and browser storage can be lost when cleared or evicted.

## [1.0.0] - 2026-11-04

### Added

- **Daily check-ins** - Record mood, optional energy, anxiety, focus, sleep, activities and notes, with one entry per calendar day.
- **Journal history** - Revisit and edit entries or permanently delete an entry after confirmation.
- **Personal insights** - Explore deterministic 7-, 30- and 90-day averages, mood charts, streaks and mood extremes from recorded data.
- **Onboarding and preferences** - Understand local-only storage, choose optional metrics, manage custom activities and switch appearance.
- **Local data controls** - Download JSON or spreadsheet-safe CSV, or explicitly delete local journal data and reset preferences.
- **Offline PWA** - Install the app and continue using it offline after an initial online visit.
- **Optional reminders** - Receive best-effort reminders while the app remains open, with optional browser notifications.

### Changed

- **Check-in controls** - Choose mood through an expressive face and an eleven-step slider; optional metrics have clearable scales and sleep has half-hour controls.
- **Product foundation** - Deliver the application as .NET 10 Blazor WebAssembly with MudBlazor, replacing the earlier Flutter prototype.

### Fixed

- **Insights layout** - Add consistent panel spacing and responsive statistic cards; prevent chart clipping on narrow screens.
- **Historical entries** - Keep recorded metrics visible even when disabled for future check-ins.
- **Data reset** - Clear onboarding, reminder and install-dismissal markers when deleting all data.
- **Asset updates** - Revalidate non-fingerprinted assets so new deployments can refresh cached styles and scripts.

### Release limitations

English only. No accounts, cloud synchronization, encrypted storage, import/restore or reliable closed-app reminders. Browser storage belongs to its origin and can be lost when cleared or evicted. A GitHub release does not confirm deployment or custom-domain readiness.

[1.2.0]: https://github.com/kasuken/DayPulse/releases/tag/v1.2.0
[1.1.0]: https://github.com/kasuken/DayPulse/releases/tag/v1.1.0
[1.0.0]: https://github.com/kasuken/DayPulse/releases/tag/v1.0.0
