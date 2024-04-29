using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class RoleCreatedHandler : IHandler<RoleCreated> {
    readonly IGatewaySender sender;

    public RoleCreatedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(RoleCreated notification, CancellationToken cancellationToken) {
        return sender.Send(notification.Room, x => x.CreateRole(notification.Room.Id, notification.Role));
    }
}
