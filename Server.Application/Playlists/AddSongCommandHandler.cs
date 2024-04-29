using Cord.Server.Application.Scrapers;
using Cord.Server.Domain.Playlists;

namespace Cord.Server.Application.Playlists;

public class AddSongCommandHandler : ICommandHandler<AddSongCommand, ISong> {
    readonly ISongRepository songRepository;
    readonly YoutubeScraper youtubeScraper;

    public AddSongCommandHandler(ISongRepository songRepository, YoutubeScraper youtubeScraper) {
        this.songRepository = songRepository;
        this.youtubeScraper = youtubeScraper;
    }

    public async Task<ISong> Handle(AddSongCommand request, CancellationToken cancellationToken) {
        if (request.Type != ImportType.Youtube) {
            throw new NotSupportedException();
        }

        var playlist = await request.User.GetPlaylist(request.PlaylistId);
        var song = await songRepository.GetByYoutubeId(request.SongId) ?? await youtubeScraper.GetSong(request.SongId);
        return await playlist.AddSong(song);
    }
}
