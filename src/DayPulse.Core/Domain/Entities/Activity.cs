namespace DayPulse.Core.Domain;

/// <summary>
/// An activity that can be associated with a check-in.
/// </summary>
public sealed class Activity
{
    private DateTimeOffset _createdAt;
    private DateTimeOffset _updatedAt;

    /// <summary>
    /// Creates an activity with the supplied values.
    /// </summary>
    /// <param name="id">The stable identifier of the activity.</param>
    /// <param name="name">The display name of the activity.</param>
    /// <param name="isDefault">Whether the activity is part of the default catalog.</param>
    /// <param name="isActive">Whether the activity can currently be selected.</param>
    /// <param name="sortOrder">The order in which the activity should be displayed.</param>
    /// <param name="createdAt">The creation timestamp. It is stored as UTC.</param>
    /// <param name="updatedAt">The last update timestamp. It is stored as UTC.</param>
    public Activity(
        string id,
        string name,
        bool isDefault = false,
        bool isActive = true,
        int sortOrder = 0,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        IsDefault = isDefault;
        IsActive = isActive;
        SortOrder = sortOrder;

        var createdTimestamp = createdAt?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
        _createdAt = createdTimestamp;
        _updatedAt = updatedAt?.ToUniversalTime() ?? createdTimestamp;
    }

    /// <summary>
    /// Gets the stable identifier of the activity.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the display name of the activity.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the activity is part of the default catalog.
    /// </summary>
    public bool IsDefault { get; }

    /// <summary>
    /// Gets a value indicating whether the activity can currently be selected.
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// Gets the display order of the activity.
    /// </summary>
    public int SortOrder { get; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt => _createdAt;

    /// <summary>
    /// Gets the UTC last-update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt => _updatedAt;
}
