namespace DayPulse.Core.Repositories;

/// <summary>
/// Indicates that a check-in date is already occupied by another check-in.
/// </summary>
public sealed class DuplicateEntryDateException : InvalidOperationException
{
    /// <summary>
    /// Creates an exception for a duplicate entry date.
    /// </summary>
    /// <param name="entryDate">The date that is already occupied.</param>
    /// <param name="existingCheckInId">The identifier of the existing check-in.</param>
    public DuplicateEntryDateException(
        DateOnly entryDate,
        string existingCheckInId)
        : base($"A check-in already exists for {entryDate:yyyy-MM-dd}.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(existingCheckInId);

        EntryDate = entryDate;
        ExistingCheckInId = existingCheckInId;
    }

    /// <summary>
    /// Gets the date that is already occupied.
    /// </summary>
    public DateOnly EntryDate { get; }

    /// <summary>
    /// Gets the identifier of the existing check-in.
    /// </summary>
    public string ExistingCheckInId { get; }
}

/// <summary>
/// Indicates that a repository entity could not be found.
/// </summary>
public sealed class RepositoryEntityNotFoundException : KeyNotFoundException
{
    /// <summary>
    /// Creates an exception for a missing repository entity.
    /// </summary>
    /// <param name="entityType">The entity type name.</param>
    /// <param name="id">The missing entity identifier.</param>
    public RepositoryEntityNotFoundException(string entityType, string id)
        : base($"{entityType} '{id}' was not found.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        EntityType = entityType;
        Id = id;
    }

    /// <summary>
    /// Gets the entity type name.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets the missing entity identifier.
    /// </summary>
    public string Id { get; }
}

/// <summary>
/// Indicates that a built-in activity cannot be hard-deleted.
/// </summary>
public sealed class BuiltInActivityDeleteException : InvalidOperationException
{
    /// <summary>
    /// Creates an exception for a protected built-in activity.
    /// </summary>
    /// <param name="activityId">The protected activity identifier.</param>
    public BuiltInActivityDeleteException(string activityId)
        : base($"Built-in activity '{activityId}' cannot be hard-deleted.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityId);
        ActivityId = activityId;
    }

    /// <summary>
    /// Gets the protected activity identifier.
    /// </summary>
    public string ActivityId { get; }
}

/// <summary>
/// Indicates that a browser storage mutation could not be committed after repeated conflicts.
/// </summary>
public sealed class RepositoryConcurrencyException : InvalidOperationException
{
    /// <summary>
    /// Creates an exception for a storage key that remained in conflict.
    /// </summary>
    /// <param name="storageKey">The storage key that could not be updated.</param>
    /// <param name="attempts">The number of attempted commits.</param>
    public RepositoryConcurrencyException(string storageKey, int attempts)
        : base($"The storage value '{storageKey}' changed during {attempts} consecutive update attempts.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(attempts);

        StorageKey = storageKey;
        Attempts = attempts;
    }

    /// <summary>
    /// Gets the storage key that could not be updated.
    /// </summary>
    public string StorageKey { get; }

    /// <summary>
    /// Gets the number of attempted commits.
    /// </summary>
    public int Attempts { get; }
}
