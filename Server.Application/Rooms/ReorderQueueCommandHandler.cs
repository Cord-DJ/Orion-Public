using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class ReorderQueueCommandHandler : ICommandHandler<ReorderQueueCommand> {
    readonly RoomProvider roomProvider;

    public ReorderQueueCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(ReorderQueueCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);
        await room.EnsureHasPermissions(request.Sender, Permission.ManageQueue);

        await room.ReorderQueue(request.Order);
        await room.SendInfoMessage($"{request.Sender.Name} has changed queue order");

        return Unit.Value;
    }
}
