using Cord.Server.Application.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

public partial class RoomsController {
    [Authorize]
    [HttpPost("{id}/users/{userId}/kick")]
    public async Task<IActionResult> Kick(ID id, ID userId) {
        await mediator.Send(new KickCommand(id, await GetSender(), userId));
        return NoContent();
    }

    [Authorize]
    [HttpGet("{id}/banned")]
    public async IAsyncEnumerable<IUser> GetBannedUsers(ID id) {
        var sender = await GetSender();
        var room = await roomProvider.GetRoom(id);
        await room.EnsureHasPermissions(sender, Permission.ManageServer);

        await room.EnsureBannedUsersLoaded();
        foreach (var x in room.Banned!) {
            yield return x;
        }
    }

    [Authorize]
    [HttpPost("{id}/users/{userId}/ban")]
    public async Task<IActionResult> Ban(ID id, ID userId, [FromBody] DurationModel model) {
        await mediator.Send(new BanCommand(id, await GetSender(), userId, model.Duration));
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}/users/{userId}/ban")]
    public async Task<IActionResult> Unban(ID id, ID userId) {
        await mediator.Send(new UnbanCommand(id, await GetSender(), userId));
        return NoContent();
    }

    [Authorize]
    [HttpGet("{id}/muted")]
    public async IAsyncEnumerable<IUser> GetMutedUsers(ID id) {
        var sender = await GetSender();
        var room = await roomProvider.GetRoom(id);
        await room.EnsureHasPermissions(sender, Permission.ManageServer);

        await room.EnsureMutedUsersLoaded();
        foreach (var x in room.Muted!) {
            yield return x;
        }
    }

    [Authorize]
    [HttpPost("{id}/users/{userId}/mute")]
    public async Task<IActionResult> Mute(ID id, ID userId, [FromBody] DurationModel model) {
        await mediator.Send(new MuteCommand(id, await GetSender(), userId, model.Duration));
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}/users/{userId}/mute")]
    public async Task<IActionResult> Unmute(ID id, ID userId) {
        await mediator.Send(new UnmuteCommand(id, await GetSender(), userId));
        return NoContent();
    }
    
    // Quick and dirty implementation of Admin-only bots
    
    [Authorize]
    [HttpPost("{id}/users/{userId}/add-bot")]
    public async Task<IActionResult> AddBot(ID id, ID userId) {
        var sender = await GetSender();
        if (!sender.HasProperties(UserProperties.Staff)) {
            return NotFound();
        }

        var bot = await userProvider.GetUser(userId);
        var room = await roomProvider.GetRoom(id);

        await bot.SetActiveRoom(room, true);
        return NoContent();
    } 
}

public record DurationModel(int? Duration);
