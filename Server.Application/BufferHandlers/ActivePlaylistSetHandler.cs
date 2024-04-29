using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class ActivePlaylistSetHandler : IHandler<ActivePlaylistSet> {
    readonly IGatewaySender sender;

    public ActivePlaylistSetHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(ActivePlaylistSet notification, CancellationToken cancellationToken) {
        return sender.Send(notification.User, x => x.UpdateUser(notification.User));
    }
}
