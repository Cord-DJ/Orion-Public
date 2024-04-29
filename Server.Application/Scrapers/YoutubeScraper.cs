using App.Metrics;
using Cord.Server.Domain;
using Cord.Server.Domain.Playlists;
using YoutubeExplode;

namespace Cord.Server.Application.Scrapers;

public class YoutubeScraper : IScraper {
    readonly ISongRepository songRepository;
    readonly IMetrics metrics;
    readonly YoutubeClient client = new();

    public YoutubeScraper(ISongRepository songRepository, IMetrics metrics) {
        this.songRepository = songRepository;
        this.metrics = metrics;
    }

    public async IAsyncEnumerable<ISong> GetSongsFromPlaylist(string id) {
        await foreach (var video in client.Playlists.GetVideosAsync(id)) {
            var existing = await songRepository.GetByYoutubeId(video.Id);
            if (existing != null) {
                yield return existing;
            } else {
                var song = new Song(
                    ID.NewId(),
                    video.Id,
                    video.Author.ChannelTitle,
                    video.Title,
                    video.Duration ?? TimeSpan.Zero
                );

                await songRepository.Add(song);
                yield return song;
            }
        }
    }

    public async Task<ISong> GetSong(string id) {
        var existing = await songRepository.GetByYoutubeId(id);
        if (existing != null) {
            return existing;
        }

        using (metrics.Measure.Timer.Time(MetricRegistry.YoutubeSongInfo)) {
            var ret = await client.Videos.GetAsync(id);
            var song = new Song(ID.NewId(), id, ret.Author.ChannelTitle, ret.Title, ret.Duration ?? TimeSpan.Zero);

            await songRepository.Add(song);
            return song;
        }
    }

    public async Task<ISong> FindSongByName(string name) {
        var video = await client.Search.GetVideosAsync(name).FirstAsync();
        var existing = await songRepository.GetByYoutubeId(video.Id);

        if (existing != null) {
            return existing;
        }

        var song = new Song(
            ID.NewId(),
            video.Id,
            video.Author.ChannelTitle,
            video.Title,
            video.Duration ?? TimeSpan.Zero
        );

        await songRepository.Add(song);
        return song;
    }

    public async Task<string> GetPlaylistName(string idOrLink) {
        var result = await client.Playlists.GetAsync(idOrLink);
        return result.Title;
    }

    public IAsyncEnumerable<(string Id, string Name)> GetUserPlaylists(string idOrLink) =>
        throw new NotImplementedException();
}
