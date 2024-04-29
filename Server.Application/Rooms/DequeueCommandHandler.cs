using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class DequeueCommandHandler : ICommandHandler<DequeueCommand> {
    readonly RoomProvider roomProvider;

    public DequeueCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(DequeueCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);

        if (request.User.Id != request.Sender.Id) {
            await room.EnsureHasPermissions(request.Sender, Permission.ManageQueue, request.User);
        }

        await room.RemoveFromQueue(request.User);
        return Unit.Value;
    }
}
