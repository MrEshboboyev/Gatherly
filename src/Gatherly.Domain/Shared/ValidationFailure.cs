namespace Gatherly.Domain.Shared;

/// <summary>
/// Represents a validation failure context with severity levels and source tracking
/// </summary>
public sealed class ValidationFailure(
    string propertyName,
    string message,
    string code = null,
    ValidationSeverity severity = ValidationSeverity.Error,
    object attemptedValue = null,
    string source = null) : IEquatable<ValidationFailure>
{
    public string PropertyName { get; } = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
    public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));
    public string Code { get; } = code ?? string.Empty;
    public ValidationSeverity Severity { get; } = severity;
    public object AttemptedValue { get; } = attemptedValue;
    public string Source { get; } = source ?? string.Empty;
    public Dictionary<string, object> CustomState { get; } = [];

    public Error AsError() => new(
        string.IsNullOrEmpty(Code) 
            ? $"Validation.{PropertyName}" 
            : Code,
        Message);

    public bool Equals(ValidationFailure other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return PropertyName == other.PropertyName &&
               Message == other.Message &&
               Code == other.Code &&
               Severity == other.Severity;
    }

    public override bool Equals(object obj) => obj is ValidationFailure failure && Equals(failure);

    public override int GetHashCode() => HashCode.Combine(PropertyName, Message, Code, Severity);

    public static bool operator ==(ValidationFailure left, ValidationFailure right) =>
        Equals(left, right);

    public static bool operator !=(ValidationFailure left, ValidationFailure right) =>
        !Equals(left, right);
}
