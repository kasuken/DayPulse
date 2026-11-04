using System.Text.Json;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;

namespace DayPulse.Tests;

public sealed class SettingsDataExportTests
{
    [Fact]
    public void Json_contains_actual_checkins_full_activity_catalog_and_settings()
    {
        var timestamp = new DateTimeOffset(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);
        var checkIn = new CheckIn("entry", new DateOnly(2026, 11, 3), 3,
            energy: 7, anxiety: 2, focus: 6, sleepHours: 7.5,
            activityIds: ["custom"], notes: "Private \"note\", with\nnew line", createdAt: timestamp);
        var activity = new Activity("custom", "Past activity", isActive: false, createdAt: timestamp);

        using var export = JsonDocument.Parse(SettingsDataExport.Json([checkIn], [activity],
            new AppSettings(onboardingCompleted: true, themeMode: ThemeMode.Dark, focusEnabled: false)));

        var entry = export.RootElement.GetProperty("CheckIns")[0];
        Assert.Equal("Private \"note\", with\nnew line", entry.GetProperty("Notes").GetString());
        Assert.Equal("custom", entry.GetProperty("ActivityIds")[0].GetString());
        Assert.Equal(7.5, entry.GetProperty("SleepHours").GetDouble());
        Assert.Equal(timestamp, entry.GetProperty("CreatedAt").GetDateTimeOffset());
        Assert.False(export.RootElement.GetProperty("Activities")[0].GetProperty("IsActive").GetBoolean());
        Assert.Equal("Past activity", export.RootElement.GetProperty("Activities")[0].GetProperty("Name").GetString());
        Assert.False(export.RootElement.GetProperty("Settings").GetProperty("FocusEnabled").GetBoolean());
        Assert.Equal(1, export.RootElement.GetProperty("SchemaVersion").GetInt32());
    }

    [Fact]
    public void Csv_quotes_commas_quotes_newlines_and_contains_catalog_and_settings_rows()
    {
        var csv = SettingsDataExport.Csv(
            [new CheckIn("entry", new DateOnly(2026, 11, 3), -2, sleepHours: 7.5,
                activityIds: ["custom"], notes: "A, \"quoted\"\r\nnote")],
            [new Activity("custom", "Walk, outside", isActive: false)],
            new AppSettings(themeMode: ThemeMode.Dark, energyEnabled: false));

        Assert.Contains("\"A, \"\"quoted\"\"\r\nnote\"", csv);
        Assert.Contains("\"[\"\"custom\"\"]\"", csv);
        Assert.Contains("\"Activity\",\"custom\"", csv);
        Assert.Contains("\"Walk, outside\",\"False\",\"False\"", csv);
        Assert.Contains("\"Settings\"", csv);
        Assert.Contains("\"Dark\"", csv);
        Assert.Contains("\"7.5\"", csv);
        Assert.Contains("\"-2\"", csv);
        Assert.Contains("\"2026-11-03\"", csv);
    }

    [Theory]
    [InlineData("=1+1")]
    [InlineData("+SUM(A1:A2)")]
    [InlineData("-cmd")]
    [InlineData("@formula")]
    [InlineData("  =1+1")]
    [InlineData("\tformula")]
    public void Csv_neutralizes_spreadsheet_formulas_while_json_preserves_original_text(string text)
    {
        var checkIn = new CheckIn("entry", new DateOnly(2026, 11, 3), 0, notes: text);

        var csv = SettingsDataExport.Csv([checkIn], [new Activity("activity", text)], new AppSettings());
        using var json = JsonDocument.Parse(SettingsDataExport.Json([checkIn], [], new AppSettings()));

        Assert.Contains($"\"'{text}\"", csv);
        Assert.Equal(text, json.RootElement.GetProperty("CheckIns")[0].GetProperty("Notes").GetString());
    }
}
