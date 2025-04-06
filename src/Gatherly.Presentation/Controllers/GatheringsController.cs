using Gatherly.Application.Gatherings.Queries.GetGatheringById;
using Gatherly.Domain.Shared;
using Gatherly.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gatherly.Presentation.Controllers;

[Route("api/gatherings")]
public sealed class GatheringsController(ISender sender) : ApiController(sender)
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetGatheringById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetGatheringByIdQuery(id);
        Result<GatheringResponse> response = await Sender.Send(
            query,
            cancellationToken);

        return response.IsSuccess
            ? Ok(response.Value)
            : NotFound(response.Error);
    }
}
