# Changelog

Notable changes to DayPulse, following [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and semantic versioning.

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

[1.0.0]: https://github.com/kasuken/DayPulse/releases/tag/v1.0.0
