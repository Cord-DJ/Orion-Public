using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YoutubeExplode;

namespace Cord.Server.Controllers;

[Route("api/youtube")]
[ApiController]
public sealed class YoutubeController : ControllerBase {
    [HttpGet("suggest")]
    [Authorize]
    public async Task<IActionResult> Suggest(string pattern) {
        var client = new HttpClient();
        var url = $"https://clients1.google.com/complete/search?client=youtube&hl=en&ds=yt&q={pattern}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return Ok(await response.Content.ReadAsStringAsync());
    }

    [HttpGet("search")]
    [Authorize]
    public async IAsyncEnumerable<dynamic> Search(string query) {
        var youtube = new YoutubeClient();

        var max = 30;
        await foreach (var video in youtube.Search.GetVideosAsync(query)) {
            if (max-- == 0) {
                yield break;
            }

            yield return new {
                YoutubeId = video.Id.Value, Name = video.Title, Author = video.Author.ChannelTitle, video.Duration
            };
        }
    }
}
