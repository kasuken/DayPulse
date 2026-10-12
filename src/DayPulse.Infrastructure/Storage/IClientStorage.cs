namespace DayPulse.Infrastructure.Storage;

/// <summary>
/// Provides access to browser local storage as a string key/value store.
/// </summary>
public interface IClientStorage
{
    /// <summary>
    /// Reads a value from local storage.
    /// </summary>
    /// <param name="key">The local storage key.</param>
    /// <param name="cancellationToken">The token used to cancel the JavaScript call.</param>
    /// <returns>The stored value, or <see langword="null"/> when the key is absent.</returns>
    ValueTask<string?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a value to local storage.
    /// </summary>
    /// <param name="key">The local storage key.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="cancellationToken">The token used to cancel the JavaScript call.</param>
    ValueTask SetAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from local storage.
    /// </summary>
    /// <param name="key">The local storage key.</param>
    /// <param name="cancellationToken">The token used to cancel the JavaScript call.</param>
    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);
}
