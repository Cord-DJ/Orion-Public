using Cord.Server.Application.Rooms;
using Cord.Server.Application.Users;
using Cord.Server.Domain.Rooms;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

[ApiController]
[Route("api/rooms")]
public partial class RoomsController : CordControllerBase {
    readonly RoomProvider roomProvider;
    readonly IMediator mediator;

    public RoomsController(
        RoomProvider roomProvider,
        UserProvider userProvider,
        IMediator mediator
    ) : base(userProvider) {
        this.roomProvider = roomProvider;
        this.mediator = mediator;
    }

    [HttpGet]
    public async IAsyncEnumerable<IRoom> Get() {
        await foreach (var x in roomProvider.GetPopularRooms()) {
            await x.LoadMembersCount();
            yield return x;
        }
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateNew([FromBody] UpdateRoom model) {
        var room = await mediator.Send(new CreateRoomCommand(await GetSender(), model));
        return StatusCode(StatusCodes.Status201Created, new { room.Id, room.Link });
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<IRoom> Update(ID id, [FromBody] UpdateRoom model) =>
        await mediator.Send(new UpdateRoomCommand(id, await GetSender(), model));

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ID id) {
        await mediator.Send(new DeleteRoomCommand(id, await GetSender()));
        return NoContent();
    }
}
