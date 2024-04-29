using App.Metrics;
using Cord.Server.Application.Scrapers;
using Cord.Server.Domain;
using Cord.Server.Domain.Playlists;

namespace Cord.Server.Application.Playlists;

public class ImportPlaylistCommandHandler : ICommandHandler<ImportPlaylistCommand, IPlaylist> {
    readonly IMetrics metrics;
    readonly YoutubeScraper youtubeScraper;
    readonly SpotifyScraper spotifyScraper;

    public ImportPlaylistCommandHandler(
        IMetrics metrics,
        YoutubeScraper youtubeScraper,
        SpotifyScraper spotifyScraper
    ) {
        this.metrics = metrics;
        this.youtubeScraper = youtubeScraper;
        this.spotifyScraper = spotifyScraper;
    }

    public async Task<IPlaylist> Handle(ImportPlaylistCommand request, CancellationToken cancellationToken) {
        switch (request.Type) {
            case ImportType.Youtube: {
                using (metrics.Measure.Timer.Time(MetricRegistry.YoutubePlaylistImport)) {
                    var playlistName = await youtubeScraper.GetPlaylistName(request.PlaylistId);
                    var playlist = (Playlist)await request.User.CreatePlaylist(playlistName, true);

                    var songs = await youtubeScraper.GetSongsFromPlaylist(request.PlaylistId).ToListAsync();
                    await playlist.AddSongs(songs);

                    metrics.Measure.Histogram.Update(MetricRegistry.ImportedSongsPerPlaylist, songs.Count);
                    return playlist;
                }
            }
            case ImportType.Spotify: {
                var name = await spotifyScraper.GetPlaylistName(request.PlaylistId);
                var playlist = (Playlist)await request.User.CreatePlaylist(name, true);

                var songs = await spotifyScraper.GetSongsFromPlaylist(request.PlaylistId).ToListAsync();
                await playlist.AddSongs(songs.DistinctBy(x => x.Id));

                metrics.Measure.Histogram.Update(MetricRegistry.ImportedSongsPerPlaylist, songs.Count);
                return playlist;
            }
            default:
                throw new BadRequestException();
        }
    }
}
