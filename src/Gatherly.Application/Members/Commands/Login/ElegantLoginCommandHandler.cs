using Gatherly.Application.Abstractions;
using Gatherly.Application.Abstractions.Messaging;
using Gatherly.Domain.Entities;
using Gatherly.Domain.Errors;
using Gatherly.Domain.Repositories;
using Gatherly.Domain.Shared;
using Gatherly.Domain.ValueObjects;

namespace Gatherly.Application.Members.Commands.Login;

/// <summary>
/// Alternative handler implementation using LINQ-style with integrated validation
/// </summary>
internal sealed class ElegantLoginCommandHandler(
    IMemberRepository memberRepository,
    IJwtProvider jwtProvider,
    IEmailDomainValidator emailDomainValidator = null) : ICommandHandler<LoginCommand, string>
{
    private readonly LoginCommandCustomValidator _validator = new(emailDomainValidator);

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Validate and transform to ValidationResult<string>
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        // Convert to ValidationResult<string> for LINQ composition
        var typedValidationResult = validationResult.ToTypedValidationResult<string>();

        if (typedValidationResult.HasFailures)
        {
            return typedValidationResult.ToResult();
        }

        // LINQ pipeline with validation result composition
        return await (
            from emailResult in Email.Create(request.Email)
            from member in GetMemberByEmailAsync(emailResult, cancellationToken)
            from token in GenerateTokenAsync(member)
            select token
        );
    }

    private async Task<Result<Member>> GetMemberByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByEmailAsync(email, cancellationToken);
        return member is not null
            ? Result.Success(member)
            : Result.Failure<Member>(DomainErrors.Member.InvalidCredentials);
    }

    private async Task<Result<string>> GenerateTokenAsync(Member member)
    {
        return Result.Success(await jwtProvider.GenerateAsync(member));
    }
}
