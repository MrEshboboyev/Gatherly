using Gatherly.Domain.Shared;
using System.Text.Json.Serialization;

namespace Gatherly.Domain.Shared;

/// <summary>
/// Specialized result type for handling validation failures with detailed metadata
/// </summary>
public sealed class ValidationResponse : Result
{
    [JsonIgnore]
    private readonly IValidationResult _validationResult;

    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }
    public ValidationSummary Summary { get; }

    private ValidationResponse(IValidationResult validationResult)
        : base(false, IValidationResult.ValidationError)
    {
        _validationResult = validationResult;

        ValidationErrors = validationResult.Failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.Message).ToArray()
            );

        Summary = new ValidationSummary(validationResult);
    }

    public static ValidationResponse FromValidationResult(IValidationResult validationResult)
    {
        if (validationResult == null || !validationResult.HasFailures)
            throw new ArgumentException("Validation result must contain failures", nameof(validationResult));

        return new ValidationResponse(validationResult);
    }

    public static Result<TValue> FromResult<TValue>(
        Result<TValue> result, 
        IValidationResult validationResult)
    {
        if (!result.IsSuccess)
        {
            // Create a ValidationResponse but return it as Result<TValue>
            var validationResponse = FromValidationResult(validationResult);
            return Failure<TValue>(validationResponse.Error);
        }

        return result;
    }
}
