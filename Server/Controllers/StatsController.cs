using Cord.Server.Domain.Rooms;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

[Route("api/stats")]
[ApiController]
public sealed class StatsController : ControllerBase {
    [HttpGet("room-count")]
    public async Task<IActionResult> Get([FromServices] IRoomRepository roomRepository) =>
        Ok(new { PopularRoomsCount = await roomRepository.PopularRoomsCount() });
}
