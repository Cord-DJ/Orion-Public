using App.Metrics;
using Cord.Server.Domain;
using Cord.Server.Domain.Rooms;
using Unit = MediatR.Unit;

namespace Cord.Server.Application.Rooms;

public class StealCommandHandler : ICommandHandler<StealCommand> {
    readonly RoomProvider roomProvider;
    readonly IMetrics metrics;

    public StealCommandHandler(RoomProvider roomProvider, IMetrics metrics) {
        this.roomProvider = roomProvider;
        this.metrics = metrics;
    }

    public async Task<Unit> Handle(StealCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);
        var current = room.CurrentSong;

        if (current == null || current.UserId == request.Sender.Id) {
            throw new NotAllowedException();
        }

        await room.EnsureHasOnlineUser(request.Sender);

        var playlistId = request.Sender.InternalActivePlaylistId;
        if (playlistId == null) {
            throw new NotAllowedException();
        }

        var votes = await room.GetVotes();
        if (votes[Vote.Steal].Any(x => x == request.Sender.Id)) {
            throw new NotAllowedException();
        }

        var playlist = await request.Sender.GetPlaylist(request.PlaylistId);
        await playlist.AddSong(current.Song);

        await room.Vote(request.Sender, Vote.Steal);
        metrics.Measure.Counter.Increment(MetricRegistry.Steals);

        return Unit.Value;
    }
}
