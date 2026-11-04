using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using Microsoft.AspNetCore.Components;

namespace DayPulse.App.Shared;

/// <summary>Manages activity names, visibility and ordering while retaining stable identifiers.</summary>
public partial class ActivityPreferences
{
    [Inject] private IActivityRepository Repository { get; set; } = default!;
    [Inject] private ILogger<ActivityPreferences> Logger { get; set; } = default!;
    private IReadOnlyList<Activity>? _activities;
    private readonly Dictionary<string, string> _names = new(StringComparer.Ordinal);
    private string _newName = string.Empty;
    private bool _busy;
    private bool _error;
    private bool _disposed;
    private int _revision;
    private readonly CancellationTokenSource _lifetime = new();
    private Task _pending = Task.CompletedTask;

    protected override Task OnInitializedAsync() => LoadAsync();

    private static bool ValidName(string name) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 120;

    private Task LoadAsync() => ExecuteAsync(() => Task.CompletedTask);

    private Task AddAsync() => ExecuteAsync(async () =>
    {
        if (!ValidName(_newName))
        {
            return;
        }

        var current = await Repository.GetAllAsync(_lifetime.Token);
        var sortOrder = current.Count == 0 ? 0 : checked(current.Max(activity => activity.SortOrder) + 1);
        await Repository.AddAsync(new Activity(Guid.NewGuid().ToString("N"), _newName.Trim(), sortOrder: sortOrder), _lifetime.Token);
        _newName = string.Empty;
    });

    private Task RenameAsync(string id) => ExecuteAsync(async () =>
    {
        var name = _names[id].Trim();
        if (!ValidName(name))
        {
            return;
        }

        var current = await Repository.GetByIdAsync(id, _lifetime.Token)
            ?? throw new InvalidOperationException("The activity no longer exists.");
        if (current.IsDefault)
        {
            return;
        }

        await Repository.UpdateAsync(Copy(current, name, current.SortOrder), _lifetime.Token);
    });

    private Task SetActiveAsync(string id, bool active) =>
        ExecuteAsync(() => Repository.SetActiveAsync(id, active, _lifetime.Token));

    private Task MoveAsync(string id, int offset) => ExecuteAsync(async () =>
    {
        // Re-read before changing order: another page may have changed the catalog.
        var catalog = await Repository.GetAllAsync(_lifetime.Token);
        var activities = catalog.Where(activity => !activity.IsDefault).ToList();
        var firstOrder = checked(catalog.Where(activity => activity.IsDefault)
            .Select(activity => activity.SortOrder).DefaultIfEmpty(-1).Max() + 1);
        var index = activities.FindIndex(activity => activity.Id == id);
        var target = index + offset;
        if (index < 0 || target < 0 || target >= activities.Count)
        {
            return;
        }

        (activities[index], activities[target]) = (activities[target], activities[index]);
        for (var order = 0; order < activities.Count; order++)
        {
            var latest = await Repository.GetByIdAsync(activities[order].Id, _lifetime.Token)
                ?? throw new InvalidOperationException("The activity no longer exists.");
            await Repository.UpdateAsync(Copy(latest, latest.Name, checked(firstOrder + order)), _lifetime.Token);
        }
    });

    private static Activity Copy(Activity activity, string name, int order) =>
        new(activity.Id, name, activity.IsDefault, activity.IsActive, order, activity.CreatedAt);

    private Task ExecuteAsync(Func<Task> operation)
    {
        if (_busy || _disposed)
        {
            return Task.CompletedTask;
        }

        return _pending = ExecuteCoreAsync(operation);
    }

    private async Task ExecuteCoreAsync(Func<Task> operation)
    {
        _busy = true;
        _error = false;
        try
        {
            await operation();
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Logger.LogDebug("Activity operation canceled during disposal.");
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Activity preferences operation failed.");
            _error = true;
        }

        try
        {
            _activities = await Repository.GetAllAsync(_lifetime.Token);
            _names.Clear();
            foreach (var activity in _activities)
            {
                _names.Add(activity.Id, activity.Name);
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Logger.LogDebug("Activity refresh canceled during disposal.");
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Activity preferences could not be refreshed.");
            _activities = null;
            _error = true;
        }
        finally
        {
            _busy = false;
            _revision++;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        await _lifetime.CancelAsync();
        await _pending;
        _lifetime.Dispose();
    }
}
