namespace DayPulse.Core.Domain;

/// <summary>
/// A wellbeing check-in for one local calendar date.
/// </summary>
public sealed class CheckIn
{
    private DateTimeOffset _createdAt;
    private DateTimeOffset _updatedAt;
    private IReadOnlyList<string> _activityIds = [];

    /// <summary>
    /// Creates a check-in with the supplied values.
    /// </summary>
    /// <param name="id">The stable identifier of the check-in.</param>
    /// <param name="entryDate">The local calendar date represented by the check-in.</param>
    /// <param name="mood">The required mood score.</param>
    /// <param name="energy">The optional energy score.</param>
    /// <param name="anxiety">The optional anxiety score.</param>
    /// <param name="focus">The optional focus score.</param>
    /// <param name="sleepHours">The optional sleep duration in hours.</param>
    /// <param name="activityIds">The identifiers of activities selected for the check-in.</param>
    /// <param name="notes">Optional free-form notes.</param>
    /// <param name="createdAt">The creation timestamp. It is stored as UTC.</param>
    /// <param name="updatedAt">The last update timestamp. It is stored as UTC.</param>
    public CheckIn(
        string id,
        DateOnly entryDate,
        int mood,
        int? energy = null,
        int? anxiety = null,
        int? focus = null,
        double? sleepHours = null,
        IEnumerable<string>? activityIds = null,
        string? notes = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
        EntryDate = entryDate;
        Mood = mood;
        Energy = energy;
        Anxiety = anxiety;
        Focus = focus;
        SleepHours = sleepHours;
        _activityIds = new List<string>(
            activityIds ?? Array.Empty<string>()).AsReadOnly();
        Notes = notes;

        var createdTimestamp = createdAt?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
        _createdAt = createdTimestamp;
        _updatedAt = updatedAt?.ToUniversalTime() ?? createdTimestamp;
    }

    /// <summary>
    /// Gets the stable identifier of the check-in.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the local calendar date represented by the check-in.
    /// </summary>
    public DateOnly EntryDate { get; }

    /// <summary>
    /// Gets the required mood score. Valid values are -5 through 5.
    /// </summary>
    public int Mood { get; }

    /// <summary>
    /// Gets the optional energy score. Valid values are 0 through 10.
    /// </summary>
    public int? Energy { get; }

    /// <summary>
    /// Gets the optional anxiety score. Valid values are 0 through 10.
    /// </summary>
    public int? Anxiety { get; }

    /// <summary>
    /// Gets the optional focus score. Valid values are 0 through 10.
    /// </summary>
    public int? Focus { get; }

    /// <summary>
    /// Gets the optional sleep duration in hours. Valid values are 0 through 15 in half-hour increments.
    /// </summary>
    public double? SleepHours { get; }

    /// <summary>
    /// Gets the activity identifiers selected for the check-in.
    /// </summary>
    public IReadOnlyList<string> ActivityIds => _activityIds;

    /// <summary>
    /// Gets the optional notes for the check-in.
    /// </summary>
    public string? Notes { get; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt => _createdAt;

    /// <summary>
    /// Gets the UTC last-update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt => _updatedAt;
}
