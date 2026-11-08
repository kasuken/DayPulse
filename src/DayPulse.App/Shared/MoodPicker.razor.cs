using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DayPulse.App.Shared;

/// <summary>A required, explicitly chosen mood with eleven exact scores and five face shortcuts.</summary>
public partial class MoodPicker
{
    private static readonly int[] Anchors = [-5, -3, 0, 3, 5];

    /// <summary>Gets or sets the mood, or null until explicitly chosen.</summary>
    [Parameter] public int? Value { get; set; }

    /// <summary>Gets or sets the callback raised when a mood is chosen.</summary>
    [Parameter] public EventCallback<int?> ValueChanged { get; set; }

    /// <summary>Gets or sets whether interaction is disabled while saving.</summary>
    [Parameter] public bool Disabled { get; set; }

    private int DisplayValue => Value ?? 0;

    private string ValueText => Value is { } score ? ScoreText(score) : Localizer["EntryMoodNotChosen"];

    private string ScoreText(int score) => Localizer["EntryMoodValue", MoodLabel(score), SignedScore(score)];

    private string SignedScore(int score) => Format.FormatNumber(score, "+0;−0;0");

    private string MoodLabel(int score) => Localizer[score switch
    {
        -5 => "EntryMoodMinus5",
        -4 => "EntryMoodMinus4",
        -3 => "EntryMoodMinus3",
        -2 => "EntryMoodMinus2",
        -1 => "EntryMoodMinus1",
        0 => "EntryMood0",
        1 => "EntryMoodPlus1",
        2 => "EntryMoodPlus2",
        3 => "EntryMoodPlus3",
        4 => "EntryMoodPlus4",
        5 => "EntryMoodPlus5",
        _ => throw new ArgumentOutOfRangeException(nameof(score), score, null),
    }];

    private Task OnInputAsync(ChangeEventArgs args) =>
        int.TryParse(args.Value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var score)
            && score is >= -5 and <= 5
            ? SetAsync(score)
            : Task.CompletedTask;

    // Native arrows/Home/End emit input events. Enter/Space also allow explicit confirmation of neutral.
    private Task OnKeyDownAsync(KeyboardEventArgs args) =>
        args.Key is "Enter" or " " ? RecordShownValueAsync() : Task.CompletedTask;

    // A pointer/touch tap on the resting thumb emits no input event.
    private Task RecordShownValueAsync() => Value is null ? SetAsync(0) : Task.CompletedTask;

    private Task SetAsync(int score) =>
        Disabled || Value == score ? Task.CompletedTask : ValueChanged.InvokeAsync(score);

    private static string MouthPath(int score) => MoodFace.MouthPath(score);

    private static string EyePath(int score) => MoodFace.EyePath(score);
}
