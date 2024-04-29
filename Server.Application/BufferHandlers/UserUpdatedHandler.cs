using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class UserUpdatedHandler : IHandler<UserUpdated> {
    readonly IGatewaySender sender;

    public UserUpdatedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(UserUpdated notification, CancellationToken cancellationToken) {
        return sender.SendToAllVisibleUsersOfUser(notification.User, x => x.UpdateUser(notification.User));
    }
}
