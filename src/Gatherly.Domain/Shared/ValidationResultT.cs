namespace Gatherly.Domain.Shared;

/// <summary>
/// Typed validation result with value handling capabilities
/// </summary>
public sealed class ValidationResult<TValue> : ValidationResultBase, IValidationResult
{
    private readonly TValue _value;
    public bool HasValue { get; }

    private ValidationResult(
        TValue value, 
        bool hasValue, 
        IEnumerable<ValidationFailure> failures = null)
        : base(failures)
    {
        _value = value;
        HasValue = hasValue;
    }

    public TValue Value => HasValue && !HasFailures
        ? _value
        : throw new InvalidOperationException("Cannot access the value of a validation result with failures or without a value.");

    public static ValidationResult<TValue> Success(TValue value) =>
        new(value, true);

    public static ValidationResult<TValue> Success() =>
        new(default, false);

    public static ValidationResult<TValue> WithFailures(IEnumerable<ValidationFailure> failures) =>
        new(default, false, failures);

    public static ValidationResult<TValue> WithFailure(ValidationFailure failure) =>
        new(default, false, [failure]);

    public static ValidationResult<TValue> WithFailure(string propertyName, string message, string code = null, ValidationSeverity severity = ValidationSeverity.Error) =>
        WithFailure(new ValidationFailure(propertyName, message, code, severity));

    public Result<TValue> ToResult()
    {
        return HasFailures
            ? Result.Failure<TValue>(IValidationResult.ValidationError)
            : HasValue ? Result.Success(_value) : throw new InvalidOperationException("Cannot convert a validation result without a value to a success result.");
    }
}
