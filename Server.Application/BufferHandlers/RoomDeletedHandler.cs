using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class RoomDeletedHandler : IHandler<RoomDeleted> {
    readonly IGatewaySender sender;

    public RoomDeletedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(RoomDeleted notification, CancellationToken cancellationToken) {
        return sender.Send(notification.Room, x => x.DeleteRoom(notification.Room.Id));
    }
}
