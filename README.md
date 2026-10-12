# DayPulse

> **Track your wellbeing. Understand your patterns.**

DayPulse is a privacy-first mobile Progressive Web Application (PWA) for tracking daily wellbeing, built with **.NET 10**, **Blazor WebAssembly**, and **MudBlazor**, hosted on **Azure Static Web Apps**.

The application allows users to record their mood, energy, anxiety, focus, sleep, activities, and optional notes in less than 20 seconds, presenting simple trends and statistics to help users understand how their wellbeing changes over time.

---

## Key Principles & Scope

- **100% Offline & Private:** All data is stored in the browser's local storage / IndexedDB. No accounts, no servers, no trackers, no external analytics.
- **Fast Daily Check-in:** Complete a check-in in under 20 seconds on a single scrolling page.
- **Deterministic Insights:** Objective, non-clinical trends, moving averages, and streaks. No AI diagnosis, no medical claims.
- **Cross-Platform PWA:** Installs seamlessly on iOS and Android via Web App Manifest & Service Worker with install guidance.
- **Cloud Hosting:** Hosted globally on Azure Static Web Apps with CI/CD via GitHub Actions.

---

## Technology Stack

- **Framework:** .NET 10 Blazor WebAssembly
- **UI Components:** MudBlazor
- **Client Storage:** LocalStorage / IndexedDB
- **Offline / Mobile:** Progressive Web App (PWA) with Service Worker and Cache-First strategy
- **Hosting & CI/CD:** Azure Static Web Apps (ASWA) & GitHub Actions
- **Testing:** xUnit, bUnit, FluentAssertions

---

## Architecture

DayPulse follows a **feature-first clean architecture**:

```text
DayPulse/
├── src/
│   ├── DayPulse.App/          # Blazor WASM host, layout, PWA assets, routes
│   │   ├── wwwroot/           # index.html, manifest.json, service-worker.js, staticwebapp.config.json
│   │   ├── Features/          # Feature slices (Onboarding, CheckIn, History, Insights, Settings)
│   │   │   ├── [Feature]/
│   │   │   │   ├── Components/
│   │   │   │   ├── Models/
│   │   │   │   └── State/
│   │   └── Shared/            # Layouts, navigation, theme provider, install prompts
│   ├── DayPulse.Core/         # Domain models, business logic, validation, statistics engine
│   └── DayPulse.Infrastructure/ # Local storage repositories, seed data, notification interop
└── tests/
    ├── DayPulse.Core.Tests/   # Unit tests for domain, statistics, streaks, and validation
    └── DayPulse.App.Tests/    # Component tests with bUnit
```

---

## Roadmap & Milestones

Project tracking and implementation issues are organized into Epics on [GitHub Issues](https://github.com/kasuken/DayPulse/issues).
