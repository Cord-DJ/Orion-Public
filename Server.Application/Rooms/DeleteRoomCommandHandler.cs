using Cord.Server.Domain;
using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class DeleteRoomCommandHandler : ICommandHandler<DeleteRoomCommand> {
    readonly RoomProvider roomProvider;

    public DeleteRoomCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(DeleteRoomCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);

        if (room.OwnerId != request.Sender.Id && !request.Sender.HasProperties(UserProperties.Staff)) {
            throw new PermissionsException(Permission.All);
        }

        await roomProvider.DeleteRoom(request.RoomId);
        return Unit.Value;
    }
}
