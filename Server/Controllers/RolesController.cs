using Cord.Server.Application.Rooms;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

public partial class RoomsController {
    [Authorize]
    [HttpPost("{roomId}/roles")]
    public async Task<IRole> CreateRole(ID roomId) =>
        await mediator.Send(new CreateRoleCommand(roomId, await GetSender()));

    [Authorize]
    [HttpDelete("{roomId}/roles/{roleId}")]
    public async Task<IActionResult> DeleteRole(ID roomId, ID roleId) {
        await mediator.Send(new DeleteRoleCommand(roomId, await GetSender(), roleId));
        return NoContent();
    }

    [Authorize]
    [HttpGet("{roomId}/roles/{roleId}/member-ids")]
    public async Task<IEnumerable<ID>> GetRoleMembers(ID roomId, ID roleId) {
        var sender = await GetSender();
        var room = await roomProvider.GetRoom(roomId);

        await room.EnsureHasPermissions(sender, Permission.ManageRoles);
        await room.EnsureMembersLoaded();

        return room.Members?.Where(x => x.Roles.Contains(roleId)).Select(x => x.User.Id) ?? Array.Empty<ID>();
    }

    // [Authorize]
    // [HttpPatch("{roomId}/roles")]
    // public async Task ReorderRoles(ID roomId, [FromBody] ID[] ids) {
    //     var sender = await GetSender();
    //     var room = await roomProvider.GetRoom(roomId);
    //
    //     await room.EnsureHasPermissions(sender, Permission.ManageRoles);
    //     
    //     
    //     // TODO: check if reordered roles are less than sender's role
    //     await room.ReorderRoles(ids);
    // }

    [Authorize]
    [HttpPatch("{roomId}/roles/{roleId}")]
    public async Task<IRole> UpdateRole(ID roomId, ID roleId, [FromBody] UpdateRole role) =>
        await mediator.Send(new UpdateRoleCommand(roomId, await GetSender(), roleId, role));
}

public class RoleValidation : AbstractValidator<UpdateRole> {
    public RoleValidation() {
        RuleFor(x => x.Name).Length(1, 32);
        RuleFor(x => x.Color).Length(7);
    }
}
