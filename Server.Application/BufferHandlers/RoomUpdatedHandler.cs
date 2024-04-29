using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class RoomUpdatedHandler : IHandler<RoomUpdated> {
    readonly IGatewaySender sender;

    public RoomUpdatedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(RoomUpdated notification, CancellationToken cancellationToken) {
        return sender.Send(notification.Room, x => x.UpdateRoom(notification.Room));
    }
}
