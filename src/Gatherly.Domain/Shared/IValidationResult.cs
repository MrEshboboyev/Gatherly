namespace Gatherly.Domain.Shared;

/// <summary>
/// Enhanced interface for validation results with additional capabilities
/// </summary>
public interface IValidationResult
{
    public static readonly Error ValidationError = new(
        "ValidationError",
        "A validation problem occurred!");

    IReadOnlyList<ValidationFailure> Failures { get; }
    IReadOnlyList<Error> Errors { get; }
    bool HasFailures { get; }
    bool HasErrorsOfSeverity(ValidationSeverity severity);
    IEnumerable<ValidationFailure> GetFailuresForProperty(string propertyName);
    IValidationResult AddFailure(ValidationFailure failure);
    IValidationResult AddFailure(
        string propertyName, 
        string message, 
        string code = null, 
        ValidationSeverity severity = ValidationSeverity.Error);
    IValidationResult MergeWith(IValidationResult other);
}
