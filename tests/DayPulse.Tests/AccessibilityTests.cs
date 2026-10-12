namespace DayPulse.Tests;

public sealed class AccessibilityTests
{
    [Fact]
    public void Application_styles_provide_keyboard_targets_and_reduced_motion_support()
    {
        var styles = File.ReadAllText(GetRepositoryPath(
            "src/DayPulse.App/wwwroot/css/app.css"));

        Assert.Contains(".daypulse-skip-link", styles);
        Assert.Contains("min-height: 2.75rem", styles);
        Assert.Contains("min-width: 2.75rem", styles);
        Assert.Contains(".daypulse-install-prompt__actions .mud-button", styles);
        Assert.Contains("#blazor-error-ui .reload:focus-visible", styles);
        Assert.Contains("#blazor-error-ui .reload", styles);
        Assert.Contains(
            ".daypulse-install-prompt__actions .mud-button:focus-visible",
            styles);
        Assert.Contains("[data-testid=\"daypulse-theme-retry\"]:focus-visible", styles);
        Assert.Contains("var(--mud-palette-warning-text, #212121)", styles);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", styles);
        Assert.Contains("animation-duration: 0.01ms !important", styles);
        Assert.Contains("transition-duration: 0.01ms !important", styles);
        Assert.Contains("scroll-behavior: auto !important", styles);
    }

    private static string GetRepositoryPath(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "DayPulse.sln")))
            {
                return Path.Combine(
                    directory.FullName,
                    relativePath.Replace('/', Path.DirectorySeparatorChar));
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the repository root from {AppContext.BaseDirectory}.");
    }
}
