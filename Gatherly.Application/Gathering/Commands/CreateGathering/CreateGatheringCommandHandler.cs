using Gatherly.Domain.Repositories;
using MediatR;

namespace Gatherly.Application.Gathering.Commands.CreateGathering;

internal class CreateGatheringCommandHandler
{
    private readonly IMemberRepository _memberRepository;
    private readonly IGatheringRepository _gatheringRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGatheringCommandHandler(IMemberRepository memberRepository,
        IGatheringRepository gatheringRepository,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _gatheringRepository = gatheringRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(CreateGatheringCommand request, 
        CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetMemberByIdAsync(request.MemberId, cancellationToken);

        if (member == null)
        {
            return Unit.Value;
        }

        var gathering = Domain.Entities.Gathering.Create(
                Guid.NewGuid(),
                member,
                request.Type,
                request.Name,
                request.ScheduledAtUtc,
                request.Location,
                request.MaximumNumberOfAttendees,
                request.InvitationsValidBeforeInHours);
        _gatheringRepository.Add(gathering);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}