namespace DayPulse.Core.Domain;

/// <summary>
/// Represents one or more domain validation failures for a check-in.
/// </summary>
public sealed class CheckInValidationException : InvalidOperationException
{
    /// <summary>
    /// Creates an exception containing the supplied validation failures.
    /// </summary>
    /// <param name="errors">The validation failures.</param>
    public CheckInValidationException(IReadOnlyList<ValidationError> errors)
        : base(CreateMessage(errors))
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = new List<ValidationError>(errors).AsReadOnly();
    }

    /// <summary>
    /// Gets the validation failures that caused the exception.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    private static string CreateMessage(IReadOnlyList<ValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return errors.Count == 0
            ? "The check-in is invalid."
            : $"The check-in is invalid: {string.Join(" ", errors.Select(error => error.Message))}";
    }
}
