using App.Metrics;
using Cord.Server.Domain;
using Cord.Server.Domain.Rooms;
using Unit = MediatR.Unit;

namespace Cord.Server.Application.Rooms;

public class VoteCommandHandler : ICommandHandler<VoteCommand> {
    readonly RoomProvider roomProvider;
    readonly IMetrics metrics;

    public VoteCommandHandler(RoomProvider roomProvider, IMetrics metrics) {
        this.roomProvider = roomProvider;
        this.metrics = metrics;
    }

    public async Task<Unit> Handle(VoteCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);
        var current = room.CurrentSong;

        // Ensure has member
        await room.GetMember(request.Sender.Id);

        if (current == null || current.UserId == request.Sender.Id) {
            throw new NotAllowedException();
        }

        await room.EnsureHasOnlineUser(request.Sender);
        metrics.Measure.Counter.Increment(request.Value == "down" ? MetricRegistry.Mehs : MetricRegistry.Woots);

        await room.Vote(request.Sender, request.Value == "down" ? Vote.Downvote : Vote.Upvote);
        var votes = await room.GetVotes();
        var down = votes[Vote.Downvote];

        await room.LoadMembersCount();

        double onlineCount = (room.OnlineCount ?? 1) - 1;
        if (down.Count / onlineCount * 100 >= 60) {
            await room.SendInfoMessage("Song has been skipped due to high Meh count");
            try {
                await room.ForceNewSong();
            } catch (Exception) {
                await room.StopPlaying();
            }
        }

        return Unit.Value;
    }
}
