using System.Globalization;

namespace DayPulse.App.Shared;

/// <summary>
/// Holds the current UI culture and applies it to the active .NET execution context.
/// </summary>
public sealed class CultureState
{
    private readonly CultureSelector _cultureSelector;
    private readonly object _gate = new();
    private CultureInfo _currentCulture;

    /// <summary>
    /// Creates the culture state with the application's default culture.
    /// </summary>
    /// <param name="cultureSelector">The supported-culture selector.</param>
    public CultureState(CultureSelector cultureSelector)
    {
        _cultureSelector = cultureSelector
            ?? throw new ArgumentNullException(nameof(cultureSelector));
        _currentCulture = CultureInfo.GetCultureInfo(CultureSelector.DefaultCultureName);
        ApplyCulture(_currentCulture, raiseChanged: false);
    }

    /// <summary>
    /// Occurs after the current UI culture changes.
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Gets the culture currently used for UI strings and formatting.
    /// </summary>
    public CultureInfo CurrentCulture
    {
        get
        {
            lock (_gate)
            {
                return _currentCulture;
            }
        }
    }

    /// <summary>
    /// Applies the first supported browser locale, or the default culture when none
    /// is supported.
    /// </summary>
    /// <param name="browserLocales">The browser's preferred locale list.</param>
    /// <returns>
    /// <see langword="true"/> when the selected culture changed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool ApplyBrowserLocales(IEnumerable<string?>? browserLocales)
    {
        var culture = _cultureSelector.SelectCulture(browserLocales);
        return ApplyCulture(culture, raiseChanged: true);
    }

    private bool ApplyCulture(CultureInfo culture, bool raiseChanged)
    {
        ArgumentNullException.ThrowIfNull(culture);

        bool changed;
        lock (_gate)
        {
            changed = !string.Equals(
                _currentCulture.Name,
                culture.Name,
                StringComparison.OrdinalIgnoreCase);
            _currentCulture = culture;

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        if (changed && raiseChanged)
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        return changed;
    }
}
