using System.Globalization;

namespace DayPulse.App.Shared;

/// <summary>
/// Selects the closest supported UI culture from browser-provided locale preferences.
/// </summary>
public sealed class CultureSelector
{
    /// <summary>
    /// The culture used when the browser locale is unavailable or unsupported.
    /// </summary>
    public const string DefaultCultureName = "en-US";

    private static readonly string[] SupportedCultureNames =
    [
        DefaultCultureName,
    ];

    /// <summary>
    /// Gets the cultures currently backed by translated resources.
    /// </summary>
    public IReadOnlyList<string> SupportedCultures => SupportedCultureNames;

    /// <summary>
    /// Selects a culture from the browser's preferred locale list.
    /// </summary>
    /// <param name="browserLocales">
    /// The browser locale preferences in descending preference order.
    /// </param>
    /// <returns>A supported culture, or <see cref="DefaultCultureName"/>.</returns>
    public CultureInfo SelectCulture(IEnumerable<string?>? browserLocales)
    {
        if (browserLocales is not null)
        {
            foreach (var browserLocale in browserLocales)
            {
                var supportedCultureName = FindSupportedCultureName(browserLocale);
                if (supportedCultureName is not null)
                {
                    return CultureInfo.GetCultureInfo(supportedCultureName);
                }
            }
        }

        return CultureInfo.GetCultureInfo(DefaultCultureName);
    }

    private static string? FindSupportedCultureName(string? browserLocale)
    {
        if (string.IsNullOrWhiteSpace(browserLocale))
        {
            return null;
        }

        var normalizedLocale = browserLocale.Trim().Replace('_', '-');
        CultureInfo requestedCulture;
        try
        {
            requestedCulture = CultureInfo.GetCultureInfo(normalizedLocale);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }

        var exactCultureName = SupportedCultureNames.FirstOrDefault(
            supportedCultureName =>
                string.Equals(
                    supportedCultureName,
                    requestedCulture.Name,
                    StringComparison.OrdinalIgnoreCase));
        if (exactCultureName is not null)
        {
            return exactCultureName;
        }

        return SupportedCultureNames.FirstOrDefault(
            supportedCultureName =>
            {
                var supportedCulture = CultureInfo.GetCultureInfo(supportedCultureName);
                return string.Equals(
                    supportedCulture.TwoLetterISOLanguageName,
                    requestedCulture.TwoLetterISOLanguageName,
                    StringComparison.OrdinalIgnoreCase);
            });
    }
}
