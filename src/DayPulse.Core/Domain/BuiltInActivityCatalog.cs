namespace DayPulse.Core.Domain;

/// <summary>
/// Defines the stable built-in activity catalog.
/// </summary>
public static class BuiltInActivityCatalog
{
    private static readonly (string Id, string Name)[] Definitions =
    [
        ("default-work", "Work"),
        ("default-exercise", "Exercise"),
        ("default-walking", "Walking"),
        ("default-reading", "Reading"),
        ("default-gaming", "Gaming"),
        ("default-family", "Family"),
        ("default-friends", "Friends"),
        ("default-travel", "Travel"),
        ("default-relaxing", "Relaxing"),
        ("default-housework", "Housework"),
        ("default-cooking", "Cooking"),
        ("default-shopping", "Shopping"),
        ("default-movies", "Movies"),
        ("default-music", "Music"),
        ("default-study", "Study"),
        ("default-meditation", "Meditation"),
    ];

    /// <summary>
    /// Gets the number of built-in activities.
    /// </summary>
    public const int Count = 16;

    /// <summary>
    /// Determines whether an identifier belongs to the built-in activity catalog.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    /// <returns><see langword="true"/> when the identifier is reserved for a built-in activity.</returns>
    public static bool IsBuiltInId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return Definitions.Any(definition =>
            string.Equals(definition.Id, id, StringComparison.Ordinal));
    }

    /// <summary>
    /// Creates fresh domain instances for the built-in activities.
    /// </summary>
    /// <param name="timestamp">The UTC timestamp to use for creation and update metadata.</param>
    /// <returns>The 16 built-in activities in display order.</returns>
    public static IReadOnlyList<Activity> CreateDefaults(DateTimeOffset timestamp)
    {
        var utcTimestamp = timestamp.ToUniversalTime();

        return
        [
            .. Definitions.Select(
                (definition, index) => new Activity(
                    definition.Id,
                    definition.Name,
                    isDefault: true,
                    isActive: true,
                    sortOrder: index,
                    createdAt: utcTimestamp,
                    updatedAt: utcTimestamp)),
        ];
    }
}
