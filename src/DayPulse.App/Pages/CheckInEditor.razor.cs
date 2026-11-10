using System.Globalization;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace DayPulse.App.Pages;

public partial class CheckInEditor
{
    /// <summary>Gets or sets the identifier to edit, or null for a new check-in.</summary>
    [Parameter] public string? Id { get; set; }

    private const double SleepMin = 0;
    private const double SleepMax = 15;
    private const double SleepStep = .5;

    // Where the unrecorded sleep thumb rests and where the steppers start from.
    private const double SleepRestingHours = 7;

    private static readonly double[] SleepPresets = [6, 7, 8, 9];

    private readonly CancellationTokenSource _lifetime = new();
    private MudForm? _form;
    private ElementReference _sleepInput;
    private ElementReference _errorSummary;
    private bool _focusErrors;
    private CheckIn? _original;
    private AppSettings _settings = new();
    private IReadOnlyList<Activity> _activities = [];
    private HashSet<string> _selectedActivities = new(StringComparer.Ordinal);
    private List<string> _errors = [];
    private DateTime? _date;
    private int? _mood;
    private int? _energy;
    private int? _anxiety;
    private int? _focus;
    private double? _sleep;
    private string? _notes;
    private string? _customActivity;
    private bool _loading = true;
    private bool _loadFailed;
    private bool _missing;
    private bool _saving;
    private int _loadVersion;

    private string CancelHref => Id is null ? "/" : $"/history/{Uri.EscapeDataString(Id)}";

    protected override Task OnParametersSetAsync() => LoadAsync();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_focusErrors)
        {
            _focusErrors = false;
            await _errorSummary.FocusAsync();
        }
    }

    private async Task LoadAsync()
    {
        var version = ++_loadVersion;
        var id = Id;
        _loading = true;
        _loadFailed = false;
        _missing = false;
        _errors = [];
        try
        {
            var settings = await Settings.LoadAsync(_lifetime.Token);
            await Activities.SeedDefaultsAsync(_lifetime.Token);
            var activities = await Activities.GetAllAsync(_lifetime.Token);
            var original = id is null ? null : await Entries.GetByIdAsync(id, _lifetime.Token);
            if (version != _loadVersion) return;
            _settings = settings;
            _activities = activities;
            _original = original;
            _missing = id is not null && original is null;
            _date = original?.EntryDate.ToDateTime(TimeOnly.MinValue) ?? Clock.GetLocalNow().Date;
            _mood = original?.Mood;
            _energy = original?.Energy;
            _anxiety = original?.Anxiety;
            _focus = original?.Focus;
            _sleep = original?.SleepHours;
            _notes = original?.Notes;
            _customActivity = null;
            // Retain inactive and even deleted activity IDs that cannot be changed in the form.
            _selectedActivities = new HashSet<string>(original?.ActivityIds ?? [], StringComparer.Ordinal);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Unable to initialize the check-in editor.");
            if (version == _loadVersion) _loadFailed = true;
        }
        finally { if (version == _loadVersion) _loading = false; }
    }

    private void SelectActivity(string id, bool selected)
    {
        if (selected) _selectedActivities.Add(id);
        else _selectedActivities.Remove(id);
    }

    private string SleepRangeValue => (_sleep ?? SleepRestingHours).ToString(CultureInfo.InvariantCulture);

    private string SleepText(double hours) => Localizer["EntrySleepValue", Format.FormatNumber(hours, "0.#")];

    private string SleepValueText => _sleep is { } sleep ? SleepText(sleep) : Localizer["JournalNotRecorded"];

    private void SetSleep(ChangeEventArgs args)
    {
        // Keep the parsed value unrounded so invalid precision still reaches the domain validator.
        if (!_saving && double.TryParse(args.Value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var hours))
        {
            _sleep = Math.Clamp(hours, SleepMin, SleepMax);
        }
    }

    private void StepSleep(double delta) => SetSleepHours((_sleep ?? SleepRestingHours) + delta);

    private void SetSleepHours(double hours)
    {
        if (!_saving) _sleep = Math.Clamp(hours, SleepMin, SleepMax);
    }

    // A tap on the resting thumb fires no input event, so the click records the value that is shown.
    private void RecordShownSleep()
    {
        if (_sleep is null) SetSleepHours(SleepRestingHours);
    }

    private void OnSleepKeyDown(KeyboardEventArgs args)
    {
        if (args.Key is "Enter" or " ") RecordShownSleep();
    }

    // The clear button disappears with the value, so focus returns to the slider instead of falling to the page.
    private async Task ClearSleepAsync()
    {
        if (_saving) return;
        _sleep = null;
        await _sleepInput.FocusAsync();
    }

    private async Task AddActivityAsync()
    {
        if (_saving || string.IsNullOrWhiteSpace(_customActivity)) return;
        _saving = true;
        _errors = [];
        try
        {
            var activity = new Activity(Guid.NewGuid().ToString("N"), _customActivity.Trim(), sortOrder: _activities.Count,
                createdAt: Clock.GetUtcNow());
            await Activities.AddAsync(activity, _lifetime.Token);
            _activities = [.. _activities, activity];
            _selectedActivities.Add(activity.Id);
            _customActivity = null;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Unable to add a custom activity.");
            _errors = ["EntryActivitySaveError"];
        }
        finally
        {
            _saving = false;
            _focusErrors = _errors.Count > 0;
        }
    }

    private async Task SaveAsync()
    {
        if (_saving || _form is null) return;
        _saving = true;
        try
        {
            _errors = [];
            await _form.ValidateAsync();
            if (!_form.IsValid) _errors.Add("EntryInputInvalid");
            if (_mood is null) _errors.Add("EntryMoodRequired");
            if (_date is null) _errors.Add("EntryDateInvalid");
            if (_errors.Count > 0 || _mood is not { } mood || _date is not { } date) return;

            var entry = new CheckIn(_original?.Id ?? Guid.NewGuid().ToString("N"), DateOnly.FromDateTime(date),
                mood, _energy, _anxiety, _focus, _sleep, _selectedActivities, _notes,
                _original?.CreatedAt ?? Clock.GetUtcNow(), Clock.GetUtcNow());
            var validation = CheckInValidator.Validate(entry, DateOnly.FromDateTime(Clock.GetLocalNow().DateTime));
            if (!validation.IsValid)
            {
                _errors = validation.Errors.Select(error => $"Entry{error.Code}").Distinct().ToList();
                return;
            }

            if (_original is null) await Entries.CreateAsync(entry, _lifetime.Token);
            else await Entries.UpdateAsync(entry, _lifetime.Token);
            Navigation.NavigateTo($"/history/{Uri.EscapeDataString(entry.Id)}");
        }
        catch (DuplicateEntryDateException) { _errors = ["EntryDuplicateEntryDate"]; }
        catch (CheckInValidationException ex)
        {
            _errors = ex.Errors.Select(error => $"Entry{error.Code}").Distinct().ToList();
        }
        catch (RepositoryEntityNotFoundException) { _errors = ["JournalMissing"]; }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Unable to save a check-in.");
            _errors = ["EntrySaveError"];
        }
        finally
        {
            _saving = false;
            _focusErrors = _errors.Count > 0;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _lifetime.CancelAsync();
        _lifetime.Dispose();
    }
}
