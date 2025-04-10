using Gatherly.Application.Abstractions;
using Gatherly.Application.Abstractions.Messaging;
using Gatherly.Domain.Entities;
using Gatherly.Domain.Errors;
using Gatherly.Domain.Repositories;
using Gatherly.Domain.Shared;
using Gatherly.Domain.ValueObjects;

namespace Gatherly.Application.Members.Commands.Login;

///// <summary>
///// Handles the <see cref="LoginCommand"/> by validating credentials and generating a JWT token.
///// </summary>
//internal sealed class LoginCommandHandler(
//    IMemberRepository memberRepository,
//    IJwtProvider jwtProvider) : ICommandHandler<LoginCommand, string>
//{
//    /// <summary>
//    /// Processes the LoginCommand and logs in the user by generating a JWT.
//    /// </summary>
//    /// <param name="request">The command request containing the user's email and password.</param>
//    /// <param name="cancellationToken">Optional cancellation token.</param>
//    /// <returns>A Result containing the JWT token if successful or an error.</returns>
//    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
//    {
//        var email = request.Email;

//        // Fluent, LINQ-style composition of result pipeline
//        return await (
//            from emailResult in Email.Create(email)
//            from member in GetMemberByEmailAsync(emailResult, cancellationToken)
//            from token in GenerateTokenAsync(member)
//            select token
//        );
//    }

//    /// <summary>
//    /// Retrieves the member from the repository by email.
//    /// </summary>
//    private async Task<Result<Member>> GetMemberByEmailAsync(Email email, CancellationToken cancellationToken)
//    {
//        var member = await memberRepository.GetByEmailAsync(email, cancellationToken);
//        return member is not null
//            ? Result.Success(member)
//            : Result.Failure<Member>(DomainErrors.Member.InvalidCredentials);
//    }

//    /// <summary>
//    /// Generates a JWT token for the authenticated member.
//    /// </summary>
//    private async Task<Result<string>> GenerateTokenAsync(Member member)
//    {
//        return Result.Success(await jwtProvider.GenerateAsync(member));
//    }
//}
