using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class RoleUpdatedHandler : IHandler<RoleUpdated> {
    readonly IGatewaySender sender;

    public RoleUpdatedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(RoleUpdated notification, CancellationToken cancellationToken) {
        return sender.Send(notification.Room, x => x.UpdateRole(notification.Room.Id, notification.Role));
    }
}
