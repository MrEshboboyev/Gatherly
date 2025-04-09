namespace Gatherly.Domain.Shared;

/// <summary>
/// Provides a fluent builder for creating validation rules
/// </summary>
public class ValidationBuilder<T>(T value)
{
    private readonly T _value = value;
    private readonly List<ValidationFailure> _failures = [];

    public ValidationBuilder<T> Requires(
        Func<T, bool> predicate,
        string propertyName,
        string message,
        string code = null,
        ValidationSeverity severity = ValidationSeverity.Error)
    {
        if (!predicate(_value))
        {
            _failures.Add(new ValidationFailure(propertyName, message, code, severity, _value));
        }
        return this;
    }

    public ValidationBuilder<T> RequiresNotNull(
        Func<T, object> selector,
        string propertyName,
        string message = null,
        string code = null)
    {
        var value = selector(_value);
        if (value == null)
        {
            _failures.Add(new ValidationFailure(
                propertyName,
                message ?? $"{propertyName} cannot be null",
                code ?? $"Validation.Required.{propertyName}",
                ValidationSeverity.Error,
                null));
        }
        return this;
    }

    public ValidationBuilder<T> RequiresNotEmpty(
        Func<T, string> selector,
        string propertyName,
        string message = null,
        string code = null)
    {
        var value = selector(_value);
        if (string.IsNullOrWhiteSpace(value))
        {
            _failures.Add(new ValidationFailure(
                propertyName,
                message ?? $"{propertyName} cannot be empty",
                code ?? $"Validation.Required.{propertyName}",
                ValidationSeverity.Error,
                value));
        }
        return this;
    }

    public ValidationBuilder<T> RequiresRange<TProperty>(
        Func<T, TProperty> selector,
        TProperty min,
        TProperty max,
        string propertyName,
        string message = null,
        string code = null) where TProperty : IComparable<TProperty>
    {
        var value = selector(_value);
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            _failures.Add(new ValidationFailure(
                propertyName,
                message ?? $"{propertyName} must be between {min} and {max}",
                code ?? $"Validation.Range.{propertyName}",
                ValidationSeverity.Error,
                value));
        }
        return this;
    }

    public ValidationBuilder<T> ForEach<TItem>(
        Func<T, IEnumerable<TItem>> selector,
        string collectionName,
        Func<TItem, ValidationResult> itemValidator)
    {
        var items = selector(_value);
        if (items != null)
        {
            var index = 0;
            foreach (var item in items)
            {
                var itemResult = itemValidator(item);
                if (itemResult.HasFailures)
                {
                    foreach (var failure in itemResult.Failures)
                    {
                        _failures.Add(new ValidationFailure(
                            $"{collectionName}[{index}].{failure.PropertyName}",
                            failure.Message,
                            failure.Code,
                            failure.Severity,
                            failure.AttemptedValue));
                    }
                }
                index++;
            }
        }
        return this;
    }

    public ValidationBuilder<T> When(
        Func<T, bool> condition,
        Action<ValidationBuilder<T>> thenClause)
    {
        if (condition(_value))
        {
            thenClause(this);
        }
        return this;
    }

    public ValidationResult Build()
    {
        return _failures.Count != 0
            ? ValidationResult.WithFailures(_failures)
            : ValidationResult.Success();
    }
}
