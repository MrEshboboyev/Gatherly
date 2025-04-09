namespace Gatherly.Domain.Shared;

/// <summary>
/// Enhanced validation result implementation with fluent API and result conversion
/// </summary>
public sealed class ValidationResult : ValidationResultBase, IValidationResult
{
    private ValidationResult(IEnumerable<ValidationFailure> failures = null)
        : base(failures)
    {
    }

    public static ValidationResult Success() => new();

    #region With Failures

    public static ValidationResult WithFailures(IEnumerable<ValidationFailure> failures) =>
        new(failures);

    public static ValidationResult WithFailure(ValidationFailure failure) =>
        new([failure]);

    public static ValidationResult WithFailure(
        string propertyName, 
        string message, 
        string code = null, 
        ValidationSeverity severity = ValidationSeverity.Error) =>
        WithFailure(new ValidationFailure(propertyName, message, code, severity));

    #endregion

    #region To Result

    public Result ToResult()
    {
        return HasFailures
            ? Result.Failure(IValidationResult.ValidationError)
            : Result.Success();
    }

    public Result<TValue> ToResult<TValue>(TValue value)
    {
        return HasFailures
            ? Result.Failure<TValue>(IValidationResult.ValidationError)
            : Result.Success(value);
    }

    public ValidationResult<TValue> ToTypedValidationResult<TValue>()
    {
        return HasFailures
            ? ValidationResult<TValue>.WithFailures(Failures)
            : ValidationResult<TValue>.Success();
    }

    #endregion
}
