namespace DayPulse.Core.Domain;

/// <summary>
/// Describes one domain validation failure.
/// </summary>
/// <param name="Code">The typed validation rule identifier.</param>
/// <param name="Message">A human-readable explanation of the failure.</param>
/// <param name="PropertyName">The related domain property, when applicable.</param>
public sealed record ValidationError(
    CheckInValidationErrorCode Code,
    string Message,
    string? PropertyName = null)
{
    /// <summary>
    /// Gets the string representation of <see cref="Code"/> for storage or logging.
    /// </summary>
    public string CodeName => Code.ToString();
}
