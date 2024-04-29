using Cord.Server.Application.Playlists;
using Cord.Server.Application.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

[ApiController]
[Route("api/playlists")]
public sealed class PlaylistsController : CordControllerBase {
    readonly IMediator mediator;

    public PlaylistsController(
        UserProvider userProvider,
        IMediator mediator
    ) : base(userProvider) {
        this.mediator = mediator;
    }

    [Authorize]
    [HttpGet]
    public async Task<IEnumerable<IPlaylist>> GetPlaylist() {
        var sender = await GetSender();
        await sender.EnsurePlaylistsLoaded();

        return sender.Playlists!;
    }

    [Authorize]
    [HttpPost]
    public async Task<IPlaylist> CreatePlaylist([FromBody] CreatePlaylistModel model) =>
        await mediator.Send(new CreatePlaylistCommand(await GetSender(), model.Name));

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<IPlaylist> UpdatePlaylist(ID id, [FromBody] CreatePlaylistModel model) =>
        await mediator.Send(new UpdatePlaylistCommand(await GetSender(), id, model.Name));

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlaylist(ID id) {
        await mediator.Send(new DeletePlaylistCommand(await GetSender(), id));
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id}/active")]
    public async Task<IActionResult> SetActivePlaylist(ID id) {
        await mediator.Send(new SetActivePlaylistCommand(await GetSender(), id));
        return NoContent();
    }

    [Authorize]
    [HttpPost("import")]
    public async Task<IPlaylist> ImportPlaylist([FromBody] ImportPlaylistModel model) =>
        await mediator.Send(new ImportPlaylistCommand(await GetSender(), model.Id, model.Type));

    [Authorize]
    [HttpPut("{id}/songs")]
    public async Task<ISong> AddSong(ID id, [FromBody] AddSongModel model) =>
        await mediator.Send(new AddSongCommand(await GetSender(), id, model.YoutubeId, ImportType.Youtube));

    [Authorize]
    [HttpPatch("{id}/songs")]
    public async Task<IActionResult> ReorderSongs(ID id, [FromBody] ReorderModel model) {
        await mediator.Send(new ReorderSongsCommand(await GetSender(), id, model.Id, model.Position));
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id}/songs/{songId}/next")]
    public async Task<IActionResult> SetNextSong(ID id, ID songId) {
        await mediator.Send(new SetNextSongCommand(await GetSender(), id, songId));
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}/songs/{songId}")]
    public async Task<IActionResult> DeleteSong(ID id, ID songId) {
        await mediator.Send(new DeleteSongCommand(await GetSender(), id, songId));
        return NoContent();
    }
}

public record CreatePlaylistModel(string Name);

public record ImportPlaylistModel(string Id, ImportType Type);

public record AddSongModel(string YoutubeId);

public record ReorderModel(ID Id, int Position);
