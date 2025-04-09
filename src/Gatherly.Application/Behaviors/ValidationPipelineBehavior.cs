using FluentValidation;
using Gatherly.Domain.Shared;
using MediatR;

namespace Gatherly.Application.Behaviors;

public class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        // Create validation result and collect all failures
        var validationResult = ValidationResult.Success();

        foreach (var validator in validators)
        {
            var fluentValidationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!fluentValidationResult.IsValid)
            {
                foreach (var failure in fluentValidationResult.Errors)
                {
                    // Convert FluentValidation failures to our validation system
                    var validationFailure = new ValidationFailure(
                        propertyName: failure.PropertyName,
                        message: failure.ErrorMessage,
                        code: failure.ErrorCode,
                        severity: ConvertSeverity(failure.Severity),
                        attemptedValue: failure.AttemptedValue,
                        source: $"FluentValidation-{validator.GetType().Name}");

                    validationResult.AddFailure(validationFailure);
                }
            }
        }

        if (validationResult.HasFailures)
        {
            return CreateValidationResult<TResponse>(validationResult);
        }

        return await next(cancellationToken);
    }

    private static ValidationSeverity ConvertSeverity(Severity severity) =>
        severity switch
        {
            Severity.Error => ValidationSeverity.Error,
            Severity.Warning => ValidationSeverity.Warning,
            Severity.Info => ValidationSeverity.Info,
            _ => ValidationSeverity.Error
        };

    private static TResult CreateValidationResult<TResult>(IValidationResult validationResult)
        where TResult : Result
    {
        // Handle non-generic Result
        if (typeof(TResult) == typeof(Result))
        {
            return (ValidationResponse.FromValidationResult(validationResult) as TResult)!;
        }

        // Handle generic Result<T>
        if (typeof(TResult).IsGenericType &&
            typeof(TResult).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];

            // Create generic ValidationResult<T>
            var genericValidationResultType = typeof(ValidationResult<>).MakeGenericType(resultType);

            // Create validationResult<T>.WithFailures method
            var withFailuresMethod = genericValidationResultType.GetMethod(
                "WithFailures",
                [typeof(IEnumerable<ValidationFailure>)]);

            // Invoke the method to create ValidationResult<T> with failures
            var typedValidationResult = withFailuresMethod!.Invoke(
                null,
                [validationResult.Failures]);

            // Convert ValidationResult<T> to ValidationResponse
            var fromValidationResultMethod = typeof(ValidationResponse)
                .GetMethod("FromValidationResult", new[] { typeof(IValidationResult) });

            var response = fromValidationResultMethod!.Invoke(
                null,
                [typedValidationResult]);

            return (TResult)response!;
        }

        throw new InvalidOperationException(
            $"Cannot create validation result for type {typeof(TResult).Name}");
    }
}
