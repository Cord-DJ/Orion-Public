using App.Metrics;
using Cord.Server.Domain;
using Cord.Server.Domain.Rooms;
using Unit = MediatR.Unit;

namespace Cord.Server.Application.Rooms;

public class SkipCommandHandler : ICommandHandler<SkipCommand> {
    readonly RoomProvider roomProvider;
    readonly IMetrics metrics;

    public SkipCommandHandler(RoomProvider roomProvider, IMetrics metrics) {
        this.roomProvider = roomProvider;
        this.metrics = metrics;
    }

    public async Task<Unit> Handle(SkipCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);

        if (room.CurrentSong?.UserId != request.Sender.Id) {
            await room.EnsureHasPermissions(request.Sender, Permission.ForceSkip);
        }

        metrics.Measure.Counter.Increment(MetricRegistry.Skips);

        try {
            await room.ForceNewSong();
        } catch (Exception) {
            await room.StopPlaying();
        }

        return Unit.Value;
    }
}
