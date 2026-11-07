using System.Buffers.Binary;
using System.Text.Json;

namespace DayPulse.Tests;

public sealed class PwaAssetTests
{
    [Fact]
    public void Manifest_declares_the_DayPulse_installation_metadata()
    {
        using var manifest = JsonDocument.Parse(ReadRepositoryFile(
            "src/DayPulse.App/wwwroot/manifest.webmanifest"));
        var root = manifest.RootElement;

        Assert.Equal("DayPulse", root.GetProperty("name").GetString());
        Assert.Equal("DayPulse", root.GetProperty("short_name").GetString());
        Assert.Equal("/", root.GetProperty("id").GetString());
        Assert.Equal("/", root.GetProperty("start_url").GetString());
        Assert.Equal("/", root.GetProperty("scope").GetString());
        Assert.Equal("standalone", root.GetProperty("display").GetString());
        var theme = global::DayPulse.App.Shared.DayPulseTheme.Create();
        Assert.Equal(theme.PaletteLight.Background.Value[..7].ToLowerInvariant(),
            root.GetProperty("background_color").GetString());
        Assert.Equal(theme.PaletteLight.Primary.Value[..7].ToLowerInvariant(),
            root.GetProperty("theme_color").GetString());

        var icons = root.GetProperty("icons").EnumerateArray().ToArray();
        Assert.Contains(icons, icon => HasIconConfiguration(icon, "icon-192.png", "192x192"));
        Assert.Contains(icons, icon => HasIconConfiguration(icon, "icon-512.png", "512x512"));
    }

    [Fact]
    public void Index_document_allows_content_to_extend_into_safe_areas()
    {
        var index = ReadRepositoryFile("src/DayPulse.App/wwwroot/index.html");

        Assert.Contains("<html lang=\"en-US\">", index);
        Assert.Contains(
            "name=\"viewport\" content=\"width=device-width, initial-scale=1.0, viewport-fit=cover\"",
            index);
        Assert.Contains(
            "<div id=\"blazor-error-ui\" role=\"alert\" aria-live=\"assertive\">",
            index);
        Assert.Contains(
            "<button type=\"button\" class=\"dismiss\" aria-label=\"Dismiss error message\">",
            index);
    }

    [Theory]
    [InlineData("icon-192.png", 192)]
    [InlineData("icon-512.png", 512)]
    public void Manifest_icons_are_valid_pngs_with_expected_dimensions(string fileName, int expectedSize)
    {
        var iconPath = GetRepositoryPath($"src/DayPulse.App/wwwroot/{fileName}");
        var header = new byte[24];

        using (var stream = File.OpenRead(iconPath))
        {
            stream.ReadExactly(header);
        }

        Assert.Equal(
            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 },
            header[..8]);
        Assert.Equal(expectedSize, BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(16, 4)));
        Assert.Equal(expectedSize, BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(20, 4)));
    }

    [Theory]
    [InlineData("service-worker.js")]
    [InlineData("service-worker.published.js")]
    public void Service_workers_use_the_generated_asset_manifest(string fileName)
    {
        var serviceWorker = ReadRepositoryFile($"src/DayPulse.App/wwwroot/{fileName}");

        Assert.Contains("service-worker-assets.js", serviceWorker);
        Assert.Contains("self.assetsManifest.assets", serviceWorker);
        Assert.Contains("asset.hash", serviceWorker);
        Assert.Contains("new URL(asset.url, baseUrl)", serviceWorker);
        Assert.Contains("cache.addAll", serviceWorker);
        Assert.Contains("shouldServeIndex", serviceWorker);
        Assert.Contains("return fetch(event.request)", serviceWorker);
    }

    [Fact]
    public void Published_service_worker_uses_a_versioned_cache_first_strategy()
    {
        var serviceWorker = ReadRepositoryFile(
            "src/DayPulse.App/wwwroot/service-worker.published.js");

        Assert.Contains("self.assetsManifest.version", serviceWorker);
        Assert.Contains("const cache = await caches.open(cacheName)", serviceWorker);
        Assert.Contains("const cachedResponse = await cache.match(request)", serviceWorker);
        Assert.Contains("self.skipWaiting()", serviceWorker);
        Assert.Contains("self.clients.claim()", serviceWorker);
        Assert.Contains("caches.delete(key)", serviceWorker);
    }

    [Fact]
    public void Published_service_worker_waits_for_the_person_to_choose_the_update()
    {
        var serviceWorker = ReadRepositoryFile(
            "src/DayPulse.App/wwwroot/service-worker.published.js");
        var install = serviceWorker[serviceWorker.IndexOf("async function onInstall()", StringComparison.Ordinal)..serviceWorker.IndexOf("function onMessage", StringComparison.Ordinal)];

        Assert.DoesNotContain("skipWaiting", install);
        Assert.Contains("addEventListener('message', onMessage)", serviceWorker);
        Assert.Contains("event.data.type === 'SKIP_WAITING'", serviceWorker);
    }

    [Fact]
    public void Index_document_registers_the_service_worker_through_the_update_bootstrap()
    {
        var index = ReadRepositoryFile("src/DayPulse.App/wwwroot/index.html");
        var bootstrap = ReadRepositoryFile("src/DayPulse.App/wwwroot/js/daypulse-update-bootstrap.js");

        Assert.Contains("js/daypulse-update-bootstrap.js", index);
        Assert.DoesNotContain("serviceWorker.register", index);
        Assert.Contains(".register(\"service-worker.js\", { updateViaCache: \"none\" })", bootstrap);
        Assert.Contains("controllerchange", bootstrap);
        Assert.Contains("navigator.serviceWorker.controller", bootstrap);
    }

    [Fact]
    public void Development_service_worker_prefers_the_network_and_falls_back_offline()
    {
        var serviceWorker = ReadRepositoryFile("src/DayPulse.App/wwwroot/service-worker.js");

        Assert.Contains("cache: 'no-store'", serviceWorker);
        Assert.Contains("Network-first request failed", serviceWorker);
        Assert.Contains("throw error", serviceWorker);
    }

    [Fact]
    public void Generated_asset_manifest_includes_the_required_pwa_assets()
    {
        var wwwroot = GetBuiltWwwroot();
        var manifestPath = Path.Combine(wwwroot, "service-worker-assets.js");
        var manifestText = File.ReadAllText(manifestPath);
        var jsonStart = manifestText.IndexOf('{');
        var jsonEnd = manifestText.LastIndexOf('}');

        Assert.True(jsonStart >= 0);
        Assert.True(jsonEnd > jsonStart);

        using var manifest = JsonDocument.Parse(manifestText[jsonStart..(jsonEnd + 1)]);
        var assets = manifest.RootElement.GetProperty("assets").EnumerateArray().ToArray();
        var assetUrls = assets
            .Select(asset => asset.GetProperty("url").GetString())
            .Where(url => url is not null)
            .Cast<string>()
            .ToArray();

        Assert.Contains("index.html", assetUrls);
        Assert.Contains("manifest.webmanifest", assetUrls);
        Assert.Contains("icon-192.png", assetUrls);
        Assert.Contains("icon-512.png", assetUrls);
        Assert.Contains("icon.svg", assetUrls);
        Assert.Contains("css/app.css", assetUrls);
        Assert.Contains("DayPulse.App.styles.css", assetUrls);
        Assert.Contains("fonts/manrope-variable.ttf", assetUrls);
        Assert.Contains("fonts/OFL-Manrope.txt", assetUrls);
        Assert.Contains("js/daypulse-storage.js", assetUrls);
        Assert.Contains("js/daypulse-update-bootstrap.js", assetUrls);
        Assert.Contains("Shared/UpdatePrompt.razor.js", assetUrls);
        Assert.Contains("Shared/BrowserLocaleInitializer.razor.js", assetUrls);
        Assert.Contains("Shared/ThemeProvider.razor.js", assetUrls);
        Assert.Contains(assetUrls, url => url.StartsWith("_framework/", StringComparison.Ordinal));
        Assert.Contains(assetUrls, url => url.EndsWith(".wasm", StringComparison.Ordinal));

        Assert.All(
            assetUrls,
            assetUrl => Assert.DoesNotContain("..", assetUrl, StringComparison.Ordinal));
    }

    private static bool HasIconConfiguration(JsonElement icon, string source, string size)
    {
        var purpose = icon.GetProperty("purpose").GetString() ?? string.Empty;
        return icon.GetProperty("src").GetString() == source
            && icon.GetProperty("sizes").GetString() == size
            && purpose.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Contains("maskable", StringComparer.Ordinal);
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        return File.ReadAllText(GetRepositoryPath(relativePath));
    }

    private static string GetRepositoryPath(string relativePath)
    {
        return Path.Combine(FindRepositoryRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string GetBuiltWwwroot()
    {
#if DEBUG
        const string configuration = "Debug";
#else
        const string configuration = "Release";
#endif
        var wwwroot = GetRepositoryPath($"src/DayPulse.App/bin/{configuration}/net10.0/wwwroot");

        Assert.True(
            Directory.Exists(wwwroot),
            $"The Blazor app output was not found at {wwwroot}. Build the app before running this test.");
        return wwwroot;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "DayPulse.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the repository root from {AppContext.BaseDirectory}.");
    }
}
