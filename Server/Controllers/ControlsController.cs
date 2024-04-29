using Cord.Server.Application.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

public partial class RoomsController {
    [Authorize]
    [HttpPatch("{id}/queue")]
    public async Task<IActionResult> ReorderQueue(ID id, [FromBody] ID[] order) {
        await mediator.Send(new ReorderQueueCommand(id, await GetSender(), order));
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id}/vote/{value}")]
    public async Task<IActionResult> Vote(ID id, string value) {
        await mediator.Send(new VoteCommand(id, await GetSender(), value));
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id}/steal")]
    public async Task<IActionResult> Steal(ID id, [FromBody] StealModel model) {
        await mediator.Send(new StealCommand(id, await GetSender(), model.PlaylistId));
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id}/skip")]
    public async Task<IActionResult> SkipSong(ID id) {
        await mediator.Send(new SkipCommand(id, await GetSender()));
        return NoContent();
    }
}

public record StealModel(ID PlaylistId);
