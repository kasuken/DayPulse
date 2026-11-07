# AGENTS.md

DayPulse is a privacy-first, offline-capable wellbeing tracker: a **.NET 10 Blazor WebAssembly PWA** using **MudBlazor**, deployed to Azure Static Web Apps. There is no backend; all user data lives in the browser (IndexedDB; legacy localStorage data is migrated on first run).

## Commands

SDK is pinned in `global.json` (10.0.x, test runner `Microsoft.Testing.Platform`).

```bash
dotnet restore DayPulse.sln
dotnet build DayPulse.sln --configuration Release --no-restore   # warnings are errors
dotnet test  DayPulse.sln --configuration Release --no-build
dotnet test  tests/DayPulse.Tests -- --filter-method "*Name*"    # xUnit v3 / MTP filtering
dotnet run --project src/DayPulse.App                            # dev server
```

CI (`.github/workflows/azure-static-web-apps.yml`) runs restore → Release build → test, then publishes `src/DayPulse.App` and deploys `publish/wwwroot`. Run build + test before finishing any change.

## Layout and dependency rules

```text
src/DayPulse.Core            Domain entities, validation, repository interfaces, DateUtils. No Blazor/JS/storage deps.
src/DayPulse.Infrastructure  Browser repositories + IndexedDB-backed storage (IAtomicClientStorage via wwwroot/js/daypulse-storage.js, with one-time localStorage migration) + DependencyInjection.AddInfrastructure.
src/DayPulse.App             Blazor WASM host: Pages/, Layout/, Shared/ (theme, localization, install prompt), Resources/, wwwroot/.
tests/DayPulse.Tests         Single xUnit v3 + bUnit project covering all layers (Domain/, Repositories/, component tests).
```

- Dependency direction: `App → Infrastructure → Core`. Never reference Blazor, JS interop, or storage from `Core`.
- New repositories: interface in `Core/Repositories`, implementation in `Infrastructure/Repositories`, register in `AddInfrastructure`. Storage keys live in `RepositoryStorageKeys`, JSON options in `RepositoryJson`.
- Domain `DateOnly` values are calendar dates; do not convert time zones in `Core`.
- Check-in validation goes through `CheckInValidator` / `DuplicateEntryDateValidator` with typed error codes.

## Conventions

- `Nullable` and `TreatWarningsAsErrors` are on globally (`Directory.Build.props`); fix warnings, don't suppress them.
- Use primary-constructor/`ArgumentNullException.ThrowIfNull` style already present; public APIs get XML docs.
- Components: prefer `.razor` + `.razor.cs` code-behind, scoped `.razor.css`, and collocated `.razor.js` modules wrapped by a C# interop class (see `ThemeProvider*`, `InstallPrompt*`). Make interop disposable and tolerant of failure.
- Use MudBlazor components and theme tokens; palettes/typography are defined only in `Shared/DayPulseTheme.cs`. Shared shape/focus/responsive rules live in `wwwroot/css/app.css`. Don't hardcode colors.
- Fonts are self-hosted (Manrope); do not add CDN/font-service requests.

## Localization

- All user-facing strings go in `Resources/SharedResource*.resx` (add the key to **en, de, and it**) and are read via `IStringLocalizer<SharedResource>`. No hardcoded English in markup.
- Only `en-US` is enabled in `CultureSelector.SupportedCultures`; keep it so unless translations are ready.
- Format dates/numbers with `LocalizedFormatService`.
- Keep `BlazorWebAssemblyLoadAllGlobalizationData` enabled in `DayPulse.App.csproj`.
- `wwwroot/index.html` error surface is intentionally English bootstrap fallback.

## Accessibility and UX (tested)

One primary navigation landmark (side rail ≥960px, bottom nav below), skip link, visible focus, 44px touch targets, safe-area padding, `prefers-reduced-motion` support. `AccessibilityTests`, `MainLayoutTests` guard these; preserve them. Check-in, history and insights use real repository data with explicit loading, empty and error states — never fake data or invented statistics.

## Product constraints

- Privacy: no accounts, servers, analytics, trackers, or external network calls for user data.
- Insights must be deterministic and non-clinical (trends, moving averages, streaks); no AI diagnosis or medical claims.
- Target: a daily check-in in under ~20 seconds on a single scrolling page.
- Optional daily reminders are best-effort while the app remains open, not guaranteed closed-app notifications. Preserve that distinction in UI and docs.
- Browser data is origin-specific and not encrypted. Export is local JSON/CSV (no import yet); confirmed data deletion must reset reminder/install markers and onboarding preferences without claiming success after partial failure.

## PWA

`wwwroot/service-worker.js` (dev) and `service-worker.published.js` (cache-first, uses `service-worker-assets.js`), `manifest.webmanifest`, and `staticwebapp.config.json` are covered by `PwaAssetTests`. When adding static assets or routes, check the service worker and SWA config (fallback routing, headers/CSP) still work.

## Testing

- xUnit v3 + bUnit 2.x. Test `Core` logic as plain unit tests; test components with bUnit and fake `IClientStorage` (`Repositories/InMemoryClientStorage.cs`) and JS interop modes instead of real browser storage.
- Add or update tests with every behavior change; keep tests deterministic (no wall-clock dependence — inject dates).
- Don't disable, skip, or weaken tests to get green.

## Git

Work is tracked via GitHub Issues/Epics (kasuken/DayPulse). Keep PRs focused; never commit `bin/`, `obj/`, `TestResults/`, or secrets (the deploy token is a repo secret).
