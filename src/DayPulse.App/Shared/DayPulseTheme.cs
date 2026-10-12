using MudBlazor;

namespace DayPulse.App.Shared;

/// <summary>
/// Creates the centralized DayPulse visual theme.
/// </summary>
public static class DayPulseTheme
{
    private const string LightPrimary = "#256F64";
    private const string LightSecondary = "#8A5B19";
    private const string LightBackground = "#F5F7F9";
    private const string LightSurface = "#FFFFFF";
    private const string LightSunken = "#EAF0F2";
    private const string LightTextPrimary = "#182A31";
    private const string LightTextSecondary = "#52636B";
    private const string LightLine = "#D8E1E4";
    private const string LightInputLine = "#75858D";

    private const string DarkPrimary = "#85D6BD";
    private const string DarkSecondary = "#E3C181";
    private const string DarkBackground = "#121D23";
    private const string DarkSurface = "#1A2931";
    private const string DarkSunken = "#21353E";
    private const string DarkTextPrimary = "#EDF4F6";
    private const string DarkTextSecondary = "#B3C5CE";
    private const string DarkLine = "#354B56";
    private const string DarkInputLine = "#7C949F";
    private const string DarkPrimaryContrastText = "#112D27";
    private const string DarkSecondaryContrastText = "#332811";

    private static readonly string[] FontFamily = ["Manrope", "system-ui", "sans-serif"];

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
                BackgroundGray = LightSunken,
                Surface = LightSurface,
                AppbarBackground = LightSurface,
                AppbarText = LightTextPrimary,
                DrawerBackground = LightBackground,
                DrawerText = LightTextPrimary,
                DrawerIcon = LightTextSecondary,
                Warning = LightSecondary,
                WarningContrastText = Colors.Shades.White,
                Success = LightPrimary,
                SuccessContrastText = Colors.Shades.White,
                Info = "#315C89",
                InfoContrastText = Colors.Shades.White,
                Error = "#AD353E",
                ErrorContrastText = Colors.Shades.White,
                TextPrimary = LightTextPrimary,
                TextSecondary = LightTextSecondary,
                ActionDefault = LightTextSecondary,
                LinesDefault = LightLine,
                LinesInputs = LightInputLine,
                Divider = LightLine,
            },
            PaletteDark = new PaletteDark
            {
                Primary = DarkPrimary,
                PrimaryContrastText = DarkPrimaryContrastText,
                Secondary = DarkSecondary,
                SecondaryContrastText = DarkSecondaryContrastText,
                Background = DarkBackground,
                BackgroundGray = DarkSunken,
                Surface = DarkSurface,
                AppbarBackground = DarkSurface,
                AppbarText = DarkTextPrimary,
                DrawerBackground = DarkBackground,
                DrawerText = DarkTextPrimary,
                DrawerIcon = DarkTextSecondary,
                Warning = DarkSecondary,
                WarningContrastText = DarkSecondaryContrastText,
                Success = DarkPrimary,
                SuccessContrastText = DarkPrimaryContrastText,
                Info = "#A8C9ED",
                InfoContrastText = "#172D44",
                Error = "#FFB1B8",
                ErrorContrastText = "#49171D",
                TextPrimary = DarkTextPrimary,
                TextSecondary = DarkTextSecondary,
                ActionDefault = DarkTextSecondary,
                LinesDefault = DarkLine,
                LinesInputs = DarkInputLine,
                Divider = DarkLine,
            },
            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = FontFamily,
                    FontSize = "1rem",
                    LineHeight = "1.65",
                },
                H1 = new H1Typography
                {
                    FontFamily = FontFamily,
                    FontSize = "2.25rem",
                    FontWeight = "800",
                    LineHeight = "1.2",
                    LetterSpacing = "-.035em",
                },
                H2 = new H2Typography
                {
                    FontFamily = FontFamily,
                    FontSize = "1.5rem",
                    FontWeight = "700",
                    LineHeight = "1.35",
                    LetterSpacing = "-.025em",
                },
                H3 = new H3Typography
                {
                    FontFamily = FontFamily,
                    FontSize = "1.25rem",
                    FontWeight = "700",
                    LineHeight = "1.4",
                    LetterSpacing = "-.02em",
                },
                H4 = new H4Typography { FontFamily = FontFamily, FontSize = "1.125rem", FontWeight = "700" },
                H5 = new H5Typography { FontFamily = FontFamily, FontSize = "1rem", FontWeight = "700" },
                H6 = new H6Typography { FontFamily = FontFamily, FontSize = "1rem", FontWeight = "700" },
                Body1 = new Body1Typography { FontFamily = FontFamily, FontSize = "1rem", LineHeight = "1.65" },
                Body2 = new Body2Typography { FontFamily = FontFamily, FontSize = ".875rem", LineHeight = "1.65" },
                Subtitle1 = new Subtitle1Typography { FontFamily = FontFamily, FontSize = "1rem", FontWeight = "600" },
                Subtitle2 = new Subtitle2Typography { FontFamily = FontFamily, FontSize = ".875rem", FontWeight = "600" },
                Caption = new CaptionTypography { FontFamily = FontFamily, FontSize = ".8125rem", LineHeight = "1.5" },
                Overline = new OverlineTypography { FontFamily = FontFamily, TextTransform = "none" },
                Button = new ButtonTypography
                {
                    FontFamily = FontFamily,
                    FontSize = ".875rem",
                    FontWeight = "700",
                    TextTransform = "none",
                    LetterSpacing = "0",
                },
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "12px",
                DrawerWidthLeft = "240px",
                AppbarHeight = "72px",
            },
        };
    }
}
