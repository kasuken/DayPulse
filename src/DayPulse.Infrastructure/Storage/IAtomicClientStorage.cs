namespace DayPulse.Infrastructure.Storage;

/// <summary>
/// Provides conditional writes for browser storage.
/// </summary>
public interface IAtomicClientStorage : IClientStorage
{
    /// <summary>
    /// Stores a value only when the current value still equals the expected value.
    /// </summary>
    /// <param name="key">The local storage key.</param>
    /// <param name="expectedValue">
    /// The value that must currently be stored, or <see langword="null"/> when the key must be absent.
    /// </param>
    /// <param name="value">The value to store when the comparison succeeds.</param>
    /// <param name="cancellationToken">The token used to cancel the JavaScript call.</param>
    /// <returns>
    /// <see langword="true"/> when the value was stored; otherwise, <see langword="false"/>
    /// when another browser context changed the value first.
    /// </returns>
    ValueTask<bool> TrySetAsync(
        string key,
        string? expectedValue,
        string value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value only when the current value still equals the expected value.
    /// </summary>
    /// <param name="key">The local storage key.</param>
    /// <param name="expectedValue">
    /// The value that must currently be stored, or <see langword="null"/> when the key must be absent.
    /// </param>
    /// <param name="cancellationToken">The token used to cancel the JavaScript call.</param>
    /// <returns>
    /// <see langword="true"/> when the value was removed; otherwise, <see langword="false"/>
    /// when another browser context changed the value first.
    /// </returns>
    ValueTask<bool> TryRemoveAsync(
        string key,
        string? expectedValue,
        CancellationToken cancellationToken = default);
}
