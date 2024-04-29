using Cord.Server.Application.Scrapers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

[Route("api/spotify")]
[ApiController]
public sealed class SpotifyController : ControllerBase {
    readonly SpotifyScraper spotifyScraper;

    // [HttpGet("suggest")]
    // [Authorize]
    // public async Task<IActionResult> Suggest(string pattern) {
    //     var client = new HttpClient();
    //     var url = $"https://clients1.google.com/complete/search?client=youtube&hl=en&ds=yt&q={pattern}";
    //     var response = await client.GetAsync(url);
    //     response.EnsureSuccessStatusCode();
    //
    //     return Ok(await response.Content.ReadAsStringAsync());
    // }

    public SpotifyController(SpotifyScraper spotifyScraper) {
        this.spotifyScraper = spotifyScraper;
    }

    // [HttpGet("search")]
    // [Authorize]
    // public async IAsyncEnumerable<dynamic> Search(string query) {
    //     // doesn't work
    //     await foreach (var user in spotifyScraper.FindUsers(query)) {
    //         yield return new {
    //             Id = user.Id,
    //             Name = user.Name,
    //             Image = user.Image
    //         };
    //     }
    // }

    [HttpGet("user/{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserPlaylists(string id) {
        try {
            var playlists = await spotifyScraper.GetUserPlaylists(id).ToListAsync();
            return Ok(playlists.Select(x => new { x.Id, x.Name }));
        } catch {
            throw new NotFoundException("spotify user", default);
        }
    }
}
