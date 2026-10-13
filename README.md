<div align="center">

<img src="src/DayPulse.App/wwwroot/icon.svg" alt="DayPulse logo" width="96" height="96">

# DayPulse

**Track your wellbeing. Understand your patterns.**

A privacy-first, offline-capable wellbeing tracker. Built with .NET 10, Blazor WebAssembly and MudBlazor.

[![Build and deploy](https://github.com/kasuken/DayPulse/actions/workflows/azure-static-web-apps.yml/badge.svg)](https://github.com/kasuken/DayPulse/actions/workflows/azure-static-web-apps.yml)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)
![PWA](https://img.shields.io/badge/PWA-installable-5A0FC8)

</div>

DayPulse helps you record mood, energy, anxiety, focus, sleep, activities and notes in under 20 seconds, then see simple trends over time. Everything stays on your device: no accounts, no servers, no trackers.

> [!NOTE]
> DayPulse is under active development. The app shell, theming, localization, install flow and the domain and storage layers are in place. The daily check-in, history and insights screens are still on the roadmap, and they say so instead of showing demo data.

## Features

- **Private by design.** Data lives in the browser (IndexedDB / local storage). There is no backend and no analytics.
- **Fast daily check-in.** One scrolling page, designed to take under 20 seconds.
- **Honest insights.** Deterministic trends, moving averages and streaks. No AI diagnosis, no medical claims.
- **Installable and offline.** A PWA with a service worker, a web manifest and install guidance for iOS and Android.
- **Calm and accessible.** Light and dark themes, self-hosted font, keyboard focus indicators, 44px touch targets, reduced-motion support.
- **Localization-ready.** English is active; German and Italian resource templates are ready to enable.

## Getting started

### Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) (version pinned in [`global.json`](global.json))

### Run locally

```bash
git clone https://github.com/kasuken/DayPulse.git
cd DayPulse
dotnet restore DayPulse.sln
dotnet run --project src/DayPulse.App
```

Then open <http://localhost:5084> (or <https://localhost:7010> with the `https` launch profile).

> [!TIP]
> The service worker only caches aggressively in published builds. Use `dotnet publish` and serve the output to test offline behavior.

## Development

```bash
dotnet build DayPulse.sln --configuration Release   # warnings are treated as errors
dotnet test  DayPulse.sln --configuration Release   # xUnit v3 + bUnit
```

Tests cover the domain and validation rules, browser repositories, theming, localization, accessibility, install prompt and PWA assets. Contributor and coding-agent guidance is in [AGENTS.md](AGENTS.md).

## Architecture

```text
src/
├── DayPulse.App/            Blazor WebAssembly host: pages, layout, theme, localization, PWA assets
├── DayPulse.Core/           Domain entities, validation, repository interfaces (no UI or storage dependencies)
└── DayPulse.Infrastructure/ Browser storage and IndexedDB-backed repositories, DI registration
tests/
└── DayPulse.Tests/          Unit and component tests
```

Dependencies flow one way: `App → Infrastructure → Core`. The domain stays free of Blazor, JavaScript interop and storage concerns, so it is easy to test and evolve.

## Design system

Teal for actions, cool slate for structure, restrained amber for warnings, and flat surfaces. [`DayPulseTheme.cs`](src/DayPulse.App/Shared/DayPulseTheme.cs) is the source of truth for palettes and typography; shared shape, focus and responsive rules live in [`app.css`](src/DayPulse.App/wwwroot/css/app.css).

| Role | Light | Dark |
| --- | --- | --- |
| Primary | `#256F64` | `#85D6BD` |
| Secondary / warning | `#8A5B19` | `#E3C181` |
| Background | `#F5F7F9` | `#121D23` |
| Surface | `#FFFFFF` | `#1A2931` |
| Primary text | `#182A31` | `#EDF4F6` |
| Secondary text | `#52636B` | `#B3C5CE` |

Manrope is self-hosted under `wwwroot/fonts/` (SIL Open Font License), so typography works offline with no font-service requests.

## Localization

UI copy lives in [`Resources/SharedResource*.resx`](src/DayPulse.App/Resources) and is read through `IStringLocalizer<SharedResource>`. See the [localization notes](src/DayPulse.App/Resources/README.md) for how to enable additional languages.

## Deployment

Pushes to `main` build, test and publish the app to **Azure Static Web Apps** through GitHub Actions. Pull requests from the same repository get a preview environment that is closed when the PR is closed. The workflow needs the `AZURE_STATIC_WEB_APPS_API_TOKEN` repository secret. Routing fallback and security headers (including a strict Content-Security-Policy) are defined in [`staticwebapp.config.json`](src/DayPulse.App/wwwroot/staticwebapp.config.json).

## Roadmap

Work is tracked as epics on [GitHub Issues](https://github.com/kasuken/DayPulse/issues).
