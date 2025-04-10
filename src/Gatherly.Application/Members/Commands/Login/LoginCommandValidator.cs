using FluentValidation;
using System.Text.RegularExpressions;

namespace Gatherly.Application.Members.Commands.Login;

/// <summary>
/// Validator for LoginCommand using FluentValidation framework
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email address is required")
            .WithErrorCode("Login.Email.Required")
            .EmailAddress()
            .WithMessage("Email address format is invalid")
            .WithErrorCode("Login.Email.Format")
            .Must(BeRegisteredDomain)
            .WithMessage("Email domain is not supported")
            .WithErrorCode("Login.Email.Domain");
    }

    private bool BeRegisteredDomain(string email)
    {
        // Sample implementation - in a real app, you might check against a whitelist
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return false;

        var domain = email.Split('@')[1].ToLowerInvariant();

        // Add your domain validation logic here
        return domain != "tempmail.com" &&
               domain != "disposable.com" &&
               domain != "throwaway.com";
    }
}
