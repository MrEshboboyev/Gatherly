namespace Gatherly.Domain.Shared;

/// <summary>
/// Provides extension methods for working with validation results
/// </summary>
public static class ValidationResultExtensions
{
    #region Validate

    public static ValidationResult<TValue> Validate<TValue>(
        this TValue value, 
        Func<TValue, ValidationResult> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        var validationResult = validator(value);
        return validationResult.HasFailures
            ? ValidationResult<TValue>.WithFailures(validationResult.Failures)
            : ValidationResult<TValue>.Success(value);
    }

    public static async Task<ValidationResult<TValue>> ValidateAsync<TValue>(
        this TValue value, 
        Func<TValue, Task<ValidationResult>> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        var validationResult = await validator(value);
        return validationResult.HasFailures
            ? ValidationResult<TValue>.WithFailures(validationResult.Failures)
            : ValidationResult<TValue>.Success(value);
    }

    #endregion

    #region To Result

    public static Result<TValue> ToResult<TValue>(
        this ValidationResult<TValue> validationResult, 
        Func<IReadOnlyList<ValidationFailure>, Error> errorFactory = null)
    {
        if (validationResult.HasFailures)
        {
            var error = errorFactory != null
                ? errorFactory(validationResult.Failures)
                : IValidationResult.ValidationError;

            return Result.Failure<TValue>(error);
        }

        return validationResult.HasValue
            ? Result.Success(validationResult.Value)
            : Result.Failure<TValue>(Error.NullValue);
    }

    #endregion

    #region Map

    public static ValidationResult<TOut> Map<TIn, TOut>(
        this ValidationResult<TIn> source,
        Func<TIn, TOut> mapper)
    {
        if (source.HasFailures)
            return ValidationResult<TOut>.WithFailures(source.Failures);

        if (!source.HasValue)
            return ValidationResult<TOut>.Success();

        try
        {
            var result = mapper(source.Value);
            return ValidationResult<TOut>.Success(result);
        }
        catch (Exception ex)
        {
            return ValidationResult<TOut>.WithFailure("Mapping", $"Error mapping value: {ex.Message}", "Validation.MappingError");
        }
    }

    public static async Task<ValidationResult<TOut>> MapAsync<TIn, TOut>(
        this ValidationResult<TIn> source,
        Func<TIn, Task<TOut>> mapper)
    {
        if (source.HasFailures)
            return ValidationResult<TOut>.WithFailures(source.Failures);

        if (!source.HasValue)
            return ValidationResult<TOut>.Success();

        try
        {
            var result = await mapper(source.Value);
            return ValidationResult<TOut>.Success(result);
        }
        catch (Exception ex)
        {
            return ValidationResult<TOut>.WithFailure("Mapping", $"Error mapping value: {ex.Message}", "Validation.MappingError");
        }
    }

    #endregion

    #region Ensure

    public static ValidationResult<TValue> Ensure<TValue>(
        this ValidationResult<TValue> source,
        Func<TValue, bool> predicate,
        string propertyName,
        string message,
        string code = null)
    {
        if (source.HasFailures || !source.HasValue)
            return source;

        if (!predicate(source.Value))
            return source.AddFailure(propertyName, message, code) as ValidationResult<TValue>;

        return source;
    }

    #endregion

    public static ValidationResult WithErrors(this IValidationResult validationResult)
    {
        var errors = validationResult.Failures
            .Where(f => f.Severity >= ValidationSeverity.Error)
            .Select(f => f.AsError())
            .ToArray();

        return errors.Length > 0
            ? ValidationResult.Success().MergeWith(validationResult) as ValidationResult
            : ValidationResult.Success();
    }
}
