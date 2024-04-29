using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class OnlineUserRemovedHandler : IHandler<OnlineUserRemoved> {
    readonly IGatewaySender sender;

    public OnlineUserRemovedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(OnlineUserRemoved notification, CancellationToken cancellationToken) {
        return sender.Send(notification.Room, x => x.DeleteOnlineUser(notification.Room.Id, notification.Id));
    }
}
