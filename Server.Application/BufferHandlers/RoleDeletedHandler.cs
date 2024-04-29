using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class RoleDeletedHandler : IHandler<RoleDeleted> {
    readonly IGatewaySender sender;

    public RoleDeletedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(RoleDeleted notification, CancellationToken cancellationToken) {
        return sender.Send(notification.Room, x => x.DeleteRole(notification.Room.Id, notification.RoleId));
    }
}
