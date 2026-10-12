using MudBlazor;
using MudBlazor.Utilities;
using DayPulse.App.Shared;

namespace DayPulse.Tests;

public sealed class DayPulseThemeTests
{
    [Fact]
    public void Create_defines_calm_tokens_for_light_and_dark_palettes()
    {
        var theme = DayPulseTheme.Create();

        Assert.Equal("#2F6F6D", ColorValue(theme.PaletteLight.Primary));
        Assert.Equal("#8C5A00", ColorValue(theme.PaletteLight.Secondary));
        Assert.Equal("#F7F8F6", ColorValue(theme.PaletteLight.Background));
        Assert.Equal("#FFFFFF", ColorValue(theme.PaletteLight.Surface));
        Assert.Equal("#E5F0ED", ColorValue(theme.PaletteLight.AppbarBackground));
        Assert.Equal("#EDF4F1", ColorValue(theme.PaletteLight.DrawerBackground));
        Assert.Equal("#1E2B2A", ColorValue(theme.PaletteLight.TextPrimary));
        Assert.Equal("#4D605E", ColorValue(theme.PaletteLight.TextSecondary));

        Assert.Equal("#8ED0C1", ColorValue(theme.PaletteDark.Primary));
        Assert.Equal("#F0C674", ColorValue(theme.PaletteDark.Secondary));
        Assert.Equal("#101A19", ColorValue(theme.PaletteDark.Background));
        Assert.Equal("#182523", ColorValue(theme.PaletteDark.Surface));
        Assert.Equal("#162321", ColorValue(theme.PaletteDark.AppbarBackground));
        Assert.Equal("#14201E", ColorValue(theme.PaletteDark.DrawerBackground));
        Assert.Equal("#F1F5F3", ColorValue(theme.PaletteDark.TextPrimary));
        Assert.Equal("#B7C7C3", ColorValue(theme.PaletteDark.TextSecondary));
    }

    [Fact]
    public void Text_and_control_tokens_meet_wcag_aa_contrast()
    {
        var theme = DayPulseTheme.Create();
        var light = theme.PaletteLight;
        var dark = theme.PaletteDark;

        Assert.True(MeetsNormalTextContrast(light.TextPrimary, light.Background));
        Assert.True(MeetsNormalTextContrast(light.TextSecondary, light.Background));
        Assert.True(MeetsNormalTextContrast(light.AppbarText, light.AppbarBackground));
        Assert.True(MeetsNormalTextContrast(light.DrawerText, light.DrawerBackground));
        Assert.True(MeetsNormalTextContrast(light.PrimaryContrastText, light.Primary));
        Assert.True(MeetsNormalTextContrast(light.SecondaryContrastText, light.Secondary));

        Assert.True(MeetsNormalTextContrast(dark.TextPrimary, dark.Background));
        Assert.True(MeetsNormalTextContrast(dark.TextSecondary, dark.Background));
        Assert.True(MeetsNormalTextContrast(dark.AppbarText, dark.AppbarBackground));
        Assert.True(MeetsNormalTextContrast(dark.DrawerText, dark.DrawerBackground));
        Assert.True(MeetsNormalTextContrast(dark.PrimaryContrastText, dark.Primary));
        Assert.True(MeetsNormalTextContrast(dark.SecondaryContrastText, dark.Secondary));
    }

    private static bool MeetsNormalTextContrast(object foreground, object background)
    {
        return ContrastRatio(foreground, background) >= 4.5;
    }

    private static double ContrastRatio(object foreground, object background)
    {
        var foregroundLuminance = RelativeLuminance(
            ParseColor(ColorValue(foreground)));
        var backgroundLuminance = RelativeLuminance(
            ParseColor(ColorValue(background)));
        var lighter = Math.Max(foregroundLuminance, backgroundLuminance);
        var darker = Math.Min(foregroundLuminance, backgroundLuminance);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static (double Red, double Green, double Blue) ParseColor(string color)
    {
        Assert.StartsWith("#", color);
        Assert.Equal(7, color.Length);

        return (
            Convert.ToInt32(color[1..3], 16) / 255d,
            Convert.ToInt32(color[3..5], 16) / 255d,
            Convert.ToInt32(color[5..7], 16) / 255d);
    }

    private static string ColorValue(object color)
    {
        return color switch
        {
            MudColor mudColor => NormalizeHex(mudColor.Value),
            string stringColor => NormalizeHex(stringColor),
            _ => throw new Xunit.Sdk.XunitException(
                $"Unexpected MudBlazor color type: {color.GetType().FullName}."),
        };
    }

    private static string NormalizeHex(string color)
    {
        var normalized = color.StartsWith("#", StringComparison.Ordinal)
            ? color
            : $"#{color}";
        return normalized[..7].ToUpperInvariant();
    }

    private static double RelativeLuminance(
        (double Red, double Green, double Blue) color)
    {
        return 0.2126 * Linearize(color.Red)
            + 0.7152 * Linearize(color.Green)
            + 0.0722 * Linearize(color.Blue);
    }

    private static double Linearize(double channel)
    {
        return channel <= 0.04045
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }
}
