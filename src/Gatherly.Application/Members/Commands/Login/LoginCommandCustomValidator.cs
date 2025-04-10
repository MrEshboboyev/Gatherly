using FluentValidation;
using Gatherly.Domain.Shared;
using System.Text.RegularExpressions;

namespace Gatherly.Application.Members.Commands.Login;

/// <summary>
/// Custom validator for LoginCommand using our advanced validation builder
/// </summary>
public class LoginCommandCustomValidator(IEmailDomainValidator emailDomainValidator = null)
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    public ValidationResult ValidateCommand(LoginCommand command)
    {
        // Using our fluent validation builder
        return Validate.For(command)
            .RequiresNotEmpty(c => c.Email, "Email", "Email address is required", "Login.Email.Required")
            .When(c => !string.IsNullOrEmpty(c.Email), then => then
                .Requires(
                    c => IsValidEmailFormat(c.Email),
                    "Email",
                    "Email address format is invalid",
                    "Login.Email.Format")
                .Requires(
                    c => IsAllowedDomain(c.Email),
                    "Email",
                    "Email domain is not supported",
                    "Login.Email.Domain"))
            .Build();
    }

    public async Task<ValidationResult> ValidateAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        // First perform synchronous validations
        var validationResult = Validate.For(command)
            .RequiresNotEmpty(c => c.Email, "Email", "Email address is required", "Login.Email.Required")
            .When(c => !string.IsNullOrEmpty(c.Email), then => then
                .Requires(
                    c => IsValidEmailFormat(c.Email),
                    "Email",
                    "Email address format is invalid",
                    "Login.Email.Format"))
            .Build();

        // If early validations failed, no need to do async checks
        if (validationResult.HasFailures)
            return validationResult;

        // Perform async domain validation if we have a validator
        if (emailDomainValidator != null && !string.IsNullOrEmpty(command.Email))
        {
            var domainValidationResult = await emailDomainValidator.ValidateDomainAsync(
                command.Email.Split('@')[1],
                cancellationToken);

            if (!domainValidationResult)
            {
                validationResult.AddFailure(
                    "Email",
                    "Email domain is not supported or is temporarily unavailable",
                    "Login.Email.Domain");
            }
        }

        return validationResult;
    }

    private static bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        try
        {
            // Match using regex pattern for basic email validation
            return EmailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private bool IsAllowedDomain(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return false;

        var domain = email.Split('@')[1].ToLowerInvariant();

        // Blocked domains check
        if (domain == "tempmail.com" ||
            domain == "disposable.com" ||
            domain == "throwaway.com")
        {
            return false;
        }

        return true;
    }
}
