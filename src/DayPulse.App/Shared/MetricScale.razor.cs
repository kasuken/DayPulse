using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DayPulse.App.Shared;

/// <summary>
/// An optional 0–10 score recorded with a native range input. The score stays unrecorded (null) until the
/// person interacts with the scale, and can be cleared back to unrecorded.
/// </summary>
public partial class MetricScale
{
    private const int Max = 10;

    // Where the unrecorded thumb rests; tapping it without dragging records this value explicitly.
    private const int RestingValue = 5;

    private ElementReference _input;

    /// <summary>Gets or sets the unique element identifier, also used as the test identifier.</summary>
    [Parameter, EditorRequired] public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the visible label.</summary>
    [Parameter, EditorRequired] public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the meaning of the lowest score.</summary>
    [Parameter, EditorRequired] public string LowLabel { get; set; } = string.Empty;

    /// <summary>Gets or sets the meaning of the highest score.</summary>
    [Parameter, EditorRequired] public string HighLabel { get; set; } = string.Empty;

    /// <summary>Gets or sets the recorded score, or null when not recorded.</summary>
    [Parameter] public int? Value { get; set; }

    /// <summary>Gets or sets the callback raised when the score is recorded or cleared.</summary>
    [Parameter] public EventCallback<int?> ValueChanged { get; set; }

    /// <summary>Gets or sets whether the scale is read-only, for example while saving.</summary>
    [Parameter] public bool Disabled { get; set; }

    private string InputId => $"{Id}-input";

    private string LabelId => $"{Id}-label";

    private int DisplayValue => Value ?? RestingValue;

    private string ValueText => Value is { } value
        ? Localizer["EntryScoreOutOf", Format.FormatNumber(value, "0"), Format.FormatNumber(Max, "0")]
        : Localizer["JournalNotRecorded"];

    private Task OnInputAsync(ChangeEventArgs args)
    {
        if (Disabled || !int.TryParse(args.Value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var score))
        {
            return Task.CompletedTask;
        }

        return SetAsync(Math.Clamp(score, 0, Max));
    }

    // A tap on the resting thumb fires no input event, so the click records the value that is shown.
    private Task RecordShownValueAsync() => Value is null ? SetAsync(RestingValue) : Task.CompletedTask;

    private Task OnKeyDownAsync(KeyboardEventArgs args) =>
        args.Key is "Enter" or " " ? RecordShownValueAsync() : Task.CompletedTask;

    private async Task ClearAsync()
    {
        await SetAsync(null);
        await _input.FocusAsync();
    }

    private Task SetAsync(int? value)
    {
        return Disabled || value == Value ? Task.CompletedTask : ValueChanged.InvokeAsync(value);
    }
}
