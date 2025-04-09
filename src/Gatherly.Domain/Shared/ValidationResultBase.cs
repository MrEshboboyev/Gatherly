namespace Gatherly.Domain.Shared;

/// <summary>
/// Base implementation of IValidationResult with common functionality
/// </summary>
public abstract class ValidationResultBase(
    IEnumerable<ValidationFailure> failures = null) : IValidationResult
{
    private readonly List<ValidationFailure> _failures = failures?.ToList() ?? [];

    public IReadOnlyList<ValidationFailure> Failures => _failures.AsReadOnly();

    public IReadOnlyList<Error> Errors => _failures
        .Where(f => f.Severity >= ValidationSeverity.Error)
        .Select(f => f.AsError())
        .ToList()
        .AsReadOnly();

    public bool HasFailures => _failures.Count > 0;

    public bool HasErrorsOfSeverity(ValidationSeverity severity) =>
        _failures.Any(f => f.Severity >= severity);

    #region Failures

    public IEnumerable<ValidationFailure> GetFailuresForProperty(string propertyName) =>
        _failures.Where(f => f.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

    public IValidationResult AddFailure(ValidationFailure failure)
    {
        if (failure != null)
            _failures.Add(failure);
        return this;
    }

    public IValidationResult AddFailure(
        string propertyName, 
        string message, 
        string code = null, 
        ValidationSeverity severity = ValidationSeverity.Error)
    {
        var failure = new ValidationFailure(propertyName, message, code, severity);
        return AddFailure(failure);
    }

    #endregion

    public IValidationResult MergeWith(IValidationResult other)
    {
        if (other != null && other.Failures.Count > 0)
        {
            foreach (var failure in other.Failures)
            {
                AddFailure(failure);
            }
        }
        return this;
    }
}
