using System.Collections;

namespace DayPulse.Core.Domain;

/// <summary>
/// The complete result of validating a check-in.
/// </summary>
public sealed class CheckInValidationResult : IReadOnlyList<ValidationError>
{
    private readonly IReadOnlyList<ValidationError> _errors;

    internal CheckInValidationResult(IEnumerable<ValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        _errors = new List<ValidationError>(errors).AsReadOnly();
    }

    /// <summary>
    /// Gets a value indicating whether the check-in passed every rule.
    /// </summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>
    /// Gets a value indicating whether at least one rule failed.
    /// </summary>
    public bool IsInvalid => !IsValid;

    /// <summary>
    /// Gets all validation failures.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors => _errors;

    /// <inheritdoc />
    public int Count => _errors.Count;

    /// <inheritdoc />
    public ValidationError this[int index] => _errors[index];

    /// <summary>
    /// Throws a domain exception when the result contains validation failures.
    /// </summary>
    public void ThrowIfInvalid()
    {
        if (IsInvalid)
        {
            throw new CheckInValidationException(_errors);
        }
    }

    /// <inheritdoc />
    public IEnumerator<ValidationError> GetEnumerator() => _errors.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
