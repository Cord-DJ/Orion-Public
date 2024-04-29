using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class EnqueueCommandHandler : ICommandHandler<EnqueueCommand> {
    readonly RoomProvider roomProvider;

    public EnqueueCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(EnqueueCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);
        await room.EnsureHasPermissions(request.Sender, Permission.CanEnqueue);

        await room.AddToQueue(request.Sender);
        return Unit.Value;
    }
}
