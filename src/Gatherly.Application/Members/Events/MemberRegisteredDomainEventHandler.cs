using Gatherly.Application.Abstractions.Messaging;
using Gatherly.Application.Abstractions;
using Gatherly.Domain.DomainEvents;
using Gatherly.Domain.Repositories;
using Gatherly.Domain.Entities;

namespace Gatherly.Application.Members.Events;

internal sealed class MemberRegisteredDomainEventHandler(
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IEmailService emailService) : IDomainEventHandler<MemberRegisteredDomainEvent>
{
    public async Task Handle(
        MemberRegisteredDomainEvent notification,
        CancellationToken cancellationToken)
    {
        Member member = await memberRepository.GetByIdAsync(
            notification.MemberId,
            cancellationToken);
        
        if (member is null)
        {
            return;
        }

        // assign "Registered" role
        member.AssignRole(Role.Registered);

        memberRepository.Update(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        await emailService.SendWelcomeEmailAsync(member, cancellationToken);
    }
}
