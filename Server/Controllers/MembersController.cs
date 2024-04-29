using Cord.Server.Application.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

public partial class RoomsController {
    // TODO: shouldn't this be Put as JoinQueue?
    [Authorize]
    [HttpPost("{id}/members/{userId}")]
    public async Task<IActionResult> AddMember(ID id, string userId) {
        var user = await ParseUserId(userId, true);
        await mediator.Send(new AddMemberCommand(id, user));

        return NoContent();
    }

    // TODO: modify member stuff

    [Authorize]
    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(ID id, string userId) {
        var user = await ParseUserId(userId);

        await mediator.Send(new RemoveMemberCommand(id, user));
        return NoContent();
    }

    [Authorize]
    [HttpPut("{roomId}/members/{userId}/roles/{roleId}")]
    public async Task<IActionResult> AddMemberRole(ID roomId, string userId, ID roleId) {
        var sender = await GetSender();
        var user = await ParseUserId(userId);

        await mediator.Send(new AddMemberRoleCommand(roomId, sender, user, roleId));
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{roomId}/members/{userId}/roles/{roleId}")]
    public async Task<IActionResult> RemoveMemberRole(ID roomId, string userId, ID roleId) {
        var sender = await GetSender();
        var user = await ParseUserId(userId);

        await mediator.Send(new RemoveMemberRoleCommand(roomId, sender, user, roleId));
        return NotFound();
    }

    [Authorize]
    [HttpPut("{id}/members/{userId}/queue")]
    public async Task<IActionResult> Enqueue(ID id, string userId) {
        var user = await ParseUserId(userId, true);

        await mediator.Send(new EnqueueCommand(id, user));
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}/members/{userId}/queue")]
    public async Task<IActionResult> Dequeue(ID id, string userId) {
        var sender = await GetSender();
        var user = await ParseUserId(userId);

        await mediator.Send(new DequeueCommand(id, sender, user));
        return NoContent();
    }
}
