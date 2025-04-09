using System.Text.Json.Serialization;

namespace Gatherly.Domain.Shared;

/// <summary>
/// Represents an error that occurred during domain operations or validation.
/// Provides a rich context for error reporting and handling.
/// </summary>
/// <remarks>
/// Creates a new error with the specified code and message.
/// </remarks>
/// <param name="code">The error code.</param>
/// <param name="message">The error message.</param>
/// <param name="statusCode">Optional status code associated with this error.</param>
public class Error(string code, string message, int? statusCode = null) : IEquatable<Error>
{
    private static readonly Dictionary<string, Error> _knownErrors = new();

    /// <summary>
    /// Represents the absence of an error.
    /// </summary>
    public static readonly Error None = Create(string.Empty, string.Empty);

    /// <summary>
    /// Represents an error that occurs when a required value is null.
    /// </summary>
    public static readonly Error NullValue = Create("Error.NullValue", "The specified result is null");

    /// <summary>
    /// Unique error code that identifies the type of error.
    /// </summary>
    public string Code { get; } = code ?? throw new ArgumentNullException(nameof(code));

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));

    /// <summary>
    /// Optional numeric status code (e.g., for HTTP status mappings).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? StatusCode { get; } = statusCode;

    /// <summary>
    /// Optional metadata associated with this error.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, object> Metadata { get; } = [];

    /// <summary>
    /// Creates and caches standard error instances to reduce allocations.
    /// </summary>
    public static Error Create(string code, string message, int? statusCode = null)
    {
        var key = $"{code}:{message}:{statusCode}";

        if (!_knownErrors.TryGetValue(key, out var error))
        {
            error = new Error(code, message, statusCode);
            _knownErrors[key] = error;
        }

        return error;
    }

    /// <summary>
    /// Creates a validation error with a specific property name.
    /// </summary>
    public static Error ValidationError(string propertyName, string message)
    {
        return Create(
            $"Validation.{propertyName}",
            message);
    }

    /// <summary>
    /// Creates a not found error for a specific entity type.
    /// </summary>
    public static Error NotFound(string entityName, object id)
    {
        return Create(
            $"NotFound.{entityName}",
            $"The {entityName} with identifier {id} was not found.",
            404);
    }

    /// <summary>
    /// Creates a conflict error for entities that already exist.
    /// </summary>
    public static Error Conflict(string entityName, string conflictReason)
    {
        return Create(
            $"Conflict.{entityName}",
            conflictReason,
            409);
    }

    /// <summary>
    /// Creates an unauthorized access error.
    /// </summary>
    public static Error Unauthorized(string operation = "access")
    {
        return Create(
            "Unauthorized",
            $"You are not authorized to {operation} this resource.",
            401);
    }

    /// <summary>
    /// Creates a forbidden operation error.
    /// </summary>
    public static Error Forbidden(string operation = "perform this operation")
    {
        return Create(
            "Forbidden",
            $"You are forbidden to {operation}.",
            403);
    }

    /// <summary>
    /// Creates a business rule violation error.
    /// </summary>
    public static Error BusinessRule(string rule, string details)
    {
        return Create(
            $"BusinessRule.{rule}",
            details,
            422);
    }

    /// <summary>
    /// Creates a concurrency error for optimistic concurrency conflicts.
    /// </summary>
    public static Error Concurrency(string entityName)
    {
        return Create(
            $"Concurrency.{entityName}",
            $"The {entityName} has been modified by another process.",
            409);
    }

    /// <summary>
    /// Creates an error indicating the request is invalid.
    /// </summary>
    public static Error InvalidRequest(string details)
    {
        return Create(
            "InvalidRequest",
            details,
            400);
    }

    /// <summary>
    /// Creates an error with metadata attached.
    /// </summary>
    public Error WithMetadata(string key, object value)
    {
        var error = new Error(Code, Message, StatusCode);
        error.Metadata[key] = value;
        return error;
    }

    /// <summary>
    /// Creates a derived error with a more specific message.
    /// </summary>
    public Error WithDetails(string additionalDetails)
    {
        return new Error(
            Code,
            $"{Message} {additionalDetails}",
            StatusCode);
    }

    public static implicit operator string(Error error) => error.Code;

    public static bool operator ==(Error a, Error b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(Error a, Error b) => !(a == b);

    public virtual bool Equals(Error other)
    {
        if (other is null)
        {
            return false;
        }

        return Code == other.Code && Message == other.Message;
    }

    public override bool Equals(object obj) => obj is Error error && Equals(error);

    public override int GetHashCode() => HashCode.Combine(Code, Message);

    public override string ToString() => $"{Code}: {Message}";
}
