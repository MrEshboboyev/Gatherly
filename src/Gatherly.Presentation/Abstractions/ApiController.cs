using Gatherly.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gatherly.Presentation.Abstractions;

[ApiController]
public abstract class ApiController(ISender sender) : ControllerBase
{
    protected readonly ISender Sender = sender;

    protected IActionResult HandleFailure(Result result)
    {
        return result switch
        {
            { IsSuccess: true } => throw new InvalidOperationException("Cannot handle successful result as failure"),

            ValidationResponse validationResponse =>
                BadRequest(new ValidationProblemDetails
                {
                    Title = "Validation Error",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Detail = validationResponse.Error.Message,
                    Errors = validationResponse.ValidationErrors
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value)
                }),

            IValidationResult validationResult =>
                BadRequest(CreateValidationProblemDetails(validationResult)),

            _ => result.Error.Code.StartsWith("NotFound")
                ? NotFound(CreateProblemDetails(
                    "Resource Not Found",
                    StatusCodes.Status404NotFound,
                    result.Error))
                : BadRequest(CreateProblemDetails(
                    "Bad Request",
                    StatusCodes.Status400BadRequest,
                    result.Error))
        };
    }

    protected IActionResult HandleFailure<T>(Result<T> result)
    {
        return HandleFailure((Result)result);
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(IValidationResult validationResult)
    {
        // Group failures by property name
        var errors = validationResult.Failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.Message).ToArray());

        // Get critical errors first, then regular errors
        var mostSevereFailure = validationResult.Failures
            .OrderByDescending(f => f.Severity)
            .FirstOrDefault();

        var title = mostSevereFailure?.Severity >= ValidationSeverity.Critical
            ? "Critical Validation Error"
            : "Validation Error";

        return new ValidationProblemDetails(errors)
        {
            Title = title,
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Detail = "One or more validation errors occurred",
            Extensions =
            {
                { "summary", new
                    {
                        totalErrors = validationResult.Failures.Count,
                        criticalCount = validationResult.Failures.Count(f => f.Severity == ValidationSeverity.Critical),
                        errorCount = validationResult.Failures.Count(f => f.Severity == ValidationSeverity.Error),
                        warningCount = validationResult.Failures.Count(f => f.Severity == ValidationSeverity.Warning)
                    }
                }
            }
        };
    }

    private static ProblemDetails CreateProblemDetails(
        string title,
        int status,
        Error error)
    {
        return new ProblemDetails
        {
            Title = title,
            Type = $"https://tools.ietf.org/html/rfc7231#section-6.5.{status % 100}",
            Detail = error.Message,
            Status = status,
            Instance = Guid.NewGuid().ToString(),
            Extensions = { { "errorCode", error.Code } }
        };
    }

    protected CreatedResult Created<T>(T value, string actionName, object routeValues = null)
        where T : class
    {
        // Standard pattern for created responses with location header
        if (routeValues != null)
        {
            return base.Created(Url.Action(actionName, routeValues), value);
        }

        return base.Created(Url.Action(actionName), value);
    }

    protected CreatedResult Created<T>(T value, Uri locationUri)
        where T : class
    {
        return base.Created(locationUri, value);
    }

    protected ActionResult<T> Success<T>(T value)
    {
        return Ok(value);
    }

    //protected override IActionResult NoContent()
    //{
    //    return NoContentResult();
    //}

    //private NoContentResult NoContentResult()
    //{
    //    return base.NoContent();
    //}
}
