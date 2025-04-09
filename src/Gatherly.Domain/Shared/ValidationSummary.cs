namespace Gatherly.Domain.Shared;

/// <summary>
/// Summarizes validation results with statistics
/// </summary>
public class ValidationSummary(IValidationResult validationResult)
{
    public int TotalFailures { get; } = validationResult.Failures.Count;
    public int ErrorCount { get; } = validationResult.Failures.Count(f => f.Severity == ValidationSeverity.Error);
    public int WarningCount { get; } = validationResult.Failures.Count(f => f.Severity == ValidationSeverity.Warning);
    public int InfoCount { get; } = validationResult.Failures.Count(f => f.Severity == ValidationSeverity.Info);
    public int CriticalCount { get; } = validationResult.Failures.Count(f => f.Severity == ValidationSeverity.Critical);
    public IDictionary<string, int> FailuresByProperty { get; } = validationResult.Failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Count());
}
