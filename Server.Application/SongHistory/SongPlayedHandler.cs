using Cord.Server.Application.Users;
using Cord.Server.Domain;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Playlists;
using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.SongHistory;

public class SongPlayedHandler : IHandler<CurrentSongUpdated> {
    readonly IGatewaySender sender;
    readonly ISongsPlayedRepository songsPlayedRepository;
    readonly UserProvider userProvider;

    public SongPlayedHandler(
        IGatewaySender sender,
        ISongsPlayedRepository songsPlayedRepository,
        UserProvider userProvider
    ) {
        this.sender = sender;
        this.songsPlayedRepository = songsPlayedRepository;
        this.userProvider = userProvider;
    }

    public async Task Handle(CurrentSongUpdated notification, CancellationToken cancellationToken) {
        var song = notification.OldSong;

        if (song != null) {
            var songPlayedModel = new SongPlayedModel(
                ID.NewId(),
                notification.Room.Id,
                song.Song.Id,
                song.UserId,
                song.Upvotes.Count,
                song.Steals.Count,
                song.Downvotes.Count
            );
            await songsPlayedRepository.Add(songPlayedModel);

            var user = await userProvider.GetUser(song.UserId);
            await sender.Send(
                notification.Room,
                x => x.CreateSongPlayed(
                    notification.Room.Id,
                    new SongPlayed(
                        songPlayedModel.Id,
                        song.Song,
                        user,
                        song.Upvotes.Count,
                        song.Downvotes.Count,
                        song.Steals.Count
                    )
                )
            );
        }
    }
}
