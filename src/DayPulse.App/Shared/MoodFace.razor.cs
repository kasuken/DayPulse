using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace DayPulse.App.Shared;

/// <summary>A decorative face drawn for a mood score from -5 to 5.</summary>
public partial class MoodFace
{
    /// <summary>Gets or sets the mood score.</summary>
    [Parameter, EditorRequired] public int Score { get; set; }

    /// <summary>Gets the localization key of the mood label for a score.</summary>
    /// <param name="score">The mood score from -5 to 5.</param>
    public static string LabelKey(int score) => score switch
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
    };

    internal static string MouthPath(int score)
    {
        var corners = 15.5 - score * .2;
        var control = 15.5 + score * .85;
        return string.Create(CultureInfo.InvariantCulture, $"M7.5 {corners:0.##} Q12 {control:0.##} 16.5 {corners:0.##}");
    }

    // Eyes soften into upward arcs for happier scores and slope inward for lower ones.
    internal static string EyePath(int score)
    {
        var corner = 10 + Math.Abs(score) * .08;
        var control = 9.5 - score * .3;
        return string.Create(CultureInfo.InvariantCulture,
            $"M7.5 {corner:0.##} Q9 {control:0.##} 10.5 10 M13.5 10 Q15 {control:0.##} 16.5 {corner:0.##}");
    }
}
