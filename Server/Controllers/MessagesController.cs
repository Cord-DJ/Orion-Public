using Cord.Server.Application.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

public partial class RoomsController {
    [Authorize]
    [HttpPost("{roomId}/messages")]
    public async Task<IMessage> SendMessage(ID roomId, [FromBody] PostMessageModel model) =>
        await mediator.Send(new SendMessageCommand(roomId, await GetSender(), model.Message));

    [Authorize]
    [HttpDelete("{roomId}/messages/{id}")]
    public async Task<IActionResult> DeleteMessage(ID roomId, ID id) {
        await mediator.Send(new DeleteMessageCommand(roomId, await GetSender(), id));
        return NoContent();
    }
}

// "content: "<@&919022715002839100> test""
// @!  is user
// @& is role

public record PostMessageModel(string Message);
