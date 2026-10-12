namespace DayPulse.Infrastructure.Storage;

/// <summary>
/// Provides access to an IndexedDB object store as a string key/value store.
/// </summary>
public interface IIndexedDbStorage
{
    /// <summary>
    /// Reads a value from an IndexedDB object store.
    /// </summary>
    /// <param name="databaseName">The IndexedDB database name.</param>
    /// <param name="storeName">The object store name.</param>
    /// <param name="key">The object store key.</param>
    /// <param name="cancellationToken">The token used to cancel the JavaScript call.</param>
    /// <returns>The stored value, or <see langword="null"/> when the key is absent.</returns>
    ValueTask<string?> GetAsync(
        string databaseName,
        string storeName,
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a value to an IndexedDB object store.
    /// </summary>
    /// <param name="databaseName">The IndexedDB database name.</param>
    /// <param name="storeName">The object store name.</param>
    /// <param name="key">The object store key.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="cancellationToken">The token used to cancel the JavaScript call.</param>
    ValueTask SetAsync(
        string databaseName,
        string storeName,
        string key,
        string value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from an IndexedDB object store.
    /// </summary>
    /// <param name="databaseName">The IndexedDB database name.</param>
    /// <param name="storeName">The object store name.</param>
    /// <param name="key">The object store key.</param>
    /// <param name="cancellationToken">The token used to cancel the JavaScript call.</param>
    ValueTask RemoveAsync(
        string databaseName,
        string storeName,
        string key,
        CancellationToken cancellationToken = default);
}
