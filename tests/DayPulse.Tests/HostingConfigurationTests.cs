using System.Text.Json;

namespace DayPulse.Tests;

public sealed class HostingConfigurationTests
{
    [Theory]
    [InlineData("/css/*")]
    [InlineData("/js/*")]
    [InlineData("/_content/*")]
    public void Stable_asset_urls_are_revalidated_instead_of_cached_immutably(string route)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DayPulse.sln")))
        {
            directory = directory.Parent;
        }
        Assert.NotNull(directory);
        using var config = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            directory.FullName, "src", "DayPulse.App", "wwwroot", "staticwebapp.config.json")));
        var definition = Assert.Single(config.RootElement.GetProperty("routes").EnumerateArray(),
            item => item.GetProperty("route").GetString() == route);
        Assert.Equal("no-cache", definition.GetProperty("headers").GetProperty("cache-control").GetString());
    }
}
