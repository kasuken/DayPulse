using MudBlazor;

namespace DayPulse.App.Shared;

/// <summary>
/// Creates the centralized DayPulse visual theme.
/// </summary>
public static class DayPulseTheme
{
    private const string LightPrimary = "#2F6F6D";
    private const string LightSecondary = "#8C5A00";
    private const string LightBackground = "#F7F8F6";
    private const string LightSurface = "#FFFFFF";
    private const string LightAppbar = "#E5F0ED";
    private const string LightDrawer = "#EDF4F1";
    private const string LightTextPrimary = "#1E2B2A";
    private const string LightTextSecondary = "#4D605E";
    private const string LightAppbarAndDrawerText = "#163E3B";

    private const string DarkPrimary = "#8ED0C1";
    private const string DarkSecondary = "#F0C674";
    private const string DarkBackground = "#101A19";
    private const string DarkSurface = "#182523";
    private const string DarkAppbar = "#162321";
    private const string DarkDrawer = "#14201E";
    private const string DarkTextPrimary = "#F1F5F3";
    private const string DarkTextSecondary = "#B7C7C3";
    private const string DarkPrimaryContrastText = "#123432";
    private const string DarkSecondaryContrastText = "#3A2D18";

    /// <summary>
    /// Creates a new MudBlazor theme with calm, circadian-inspired light and dark palettes.
    /// </summary>
    /// <returns>A fully configured DayPulse theme.</returns>
    public static MudTheme Create()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = LightPrimary,
                PrimaryContrastText = Colors.Shades.White,
                Secondary = LightSecondary,
                SecondaryContrastText = Colors.Shades.White,
                Background = LightBackground,
                Surface = LightSurface,
                AppbarBackground = LightAppbar,
                AppbarText = LightAppbarAndDrawerText,
                DrawerBackground = LightDrawer,
                DrawerText = LightAppbarAndDrawerText,
                Warning = LightSecondary,
                WarningContrastText = Colors.Shades.White,
                TextPrimary = LightTextPrimary,
                TextSecondary = LightTextSecondary,
            },
            PaletteDark = new PaletteDark
            {
                Primary = DarkPrimary,
                PrimaryContrastText = DarkPrimaryContrastText,
                Secondary = DarkSecondary,
                SecondaryContrastText = DarkSecondaryContrastText,
                Background = DarkBackground,
                Surface = DarkSurface,
                AppbarBackground = DarkAppbar,
                AppbarText = DarkTextPrimary,
                DrawerBackground = DarkDrawer,
                DrawerText = DarkTextPrimary,
                Warning = DarkSecondary,
                WarningContrastText = DarkSecondaryContrastText,
                TextPrimary = DarkTextPrimary,
                TextSecondary = DarkTextSecondary,
            },
        };
    }
}
