namespace Gatherly.Domain.Shared;

/// <summary>
/// Factory for creating validation builders
/// </summary>
public static class Validate
{
    public static ValidationBuilder<T> For<T>(T value) => new(value);

    public static ValidationResult NotNull<T>(
        T value, 
        string propertyName, 
        string message = null)
    {
        if (value == null)
        {
            return ValidationResult.WithFailure(
                propertyName,
                message ?? $"{propertyName} cannot be null",
                $"Validation.Required.{propertyName}");
        }
        return ValidationResult.Success();
    }

    public static ValidationResult NotEmpty(
        string value, 
        string propertyName, 
        string message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.WithFailure(
                propertyName,
                message ?? $"{propertyName} cannot be empty",
                $"Validation.Required.{propertyName}");
        }
        return ValidationResult.Success();
    }

    public static ValidationResult NotEmpty<T>(
        IEnumerable<T> collection, 
        string propertyName, 
        string message = null)
    {
        if (collection == null || !collection.Any())
        {
            return ValidationResult.WithFailure(
                propertyName,
                message ?? $"{propertyName} cannot be empty",
                $"Validation.Required.{propertyName}");
        }
        return ValidationResult.Success();
    }

    public static ValidationResult Range<T>(
        T value, 
        T min, 
        T max, 
        string propertyName, 
        string message = null) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            return ValidationResult.WithFailure(
                propertyName,
                message ?? $"{propertyName} must be between {min} and {max}",
                $"Validation.Range.{propertyName}");
        }
        return ValidationResult.Success();
    }

    public static ValidationResult Email(
        string email, 
        string propertyName = "Email",
        string message = null)
    {
        // Simple email validation logic
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.'))
        {
            return ValidationResult.WithFailure(
                propertyName,
                message ?? "Invalid email address",
                "Validation.Email");
        }
        return ValidationResult.Success();
    }
}
