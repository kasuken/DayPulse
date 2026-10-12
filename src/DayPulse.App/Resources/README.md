# DayPulse localization resources

`SharedResource.resx` is the neutral English resource and is used for the initial
`en-US` culture. Components resolve strings through `IStringLocalizer<SharedResource>`
so changing a translation never requires changing component markup.

Culture-specific files use the standard resource naming convention:

```text
Resources/SharedResource.resx       # neutral English / en-US source
Resources/SharedResource.de.resx    # German translation
Resources/SharedResource.it.resx    # Italian translation
```

The German and Italian files currently mirror the English keys as translation
templates. `CultureSelector.SupportedCultures` intentionally enables only `en-US`
until those translations are ready; enabling `de` or `it` requires changing only
that supported-culture list and the corresponding resource values.

`DayPulse.App.csproj` enables `BlazorWebAssemblyLoadAllGlobalizationData` because
the browser can start WebAssembly in a different culture before `CultureState`
applies the supported English fallback. Without this setting, a German or
Italian browser can fail during startup even though the resource templates are
present. Keep it enabled when adding supported languages.

Use `LocalizedFormatService` for user-facing date, time, and numeric values. It
always formats with `CultureInfo.CurrentCulture`; domain `DateOnly` values remain
calendar dates and are not converted or localized in `DayPulse.Core`.

The fatal-error surface in `wwwroot/index.html` is an intentionally English
bootstrap fallback because it can render before Blazor, dependency injection,
and resource loading are available. `BrowserLocaleInitializer` replaces its
messages after the first render using the selected UI culture.

The reusable `.blazor-error-boundary` style uses a `data-error-message`
attribute so an application-level error boundary can use the same localized
message without embedding English in its component markup.
