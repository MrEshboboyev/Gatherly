namespace Gatherly.Application.Members.Commands.Login;

/// <summary>
/// Interface for asynchronous email domain validation
/// </summary>
public interface IEmailDomainValidator
{
    Task<bool> ValidateDomainAsync(string domain, CancellationToken cancellationToken = default);
}
