using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class PlaylistDeletedHandler : IHandler<PlaylistDeleted> {
    readonly IGatewaySender gateway;

    public PlaylistDeletedHandler(IGatewaySender gateway) {
        this.gateway = gateway;
    }

    public Task Handle(PlaylistDeleted @event, CancellationToken token = default) {
        return gateway.Send(@event.User, x => x.DeletePlaylist(@event.PlaylistId));
    }
}
