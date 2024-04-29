using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class RoomCreatedHandler : IHandler<RoomCreated> {
    readonly IGatewaySender gateway;

    public RoomCreatedHandler(IGatewaySender gateway) {
        this.gateway = gateway;
    }

    public Task Handle(RoomCreated @event, CancellationToken token = default) {
        return gateway.Send(@event.Creator, x => x.CreateRoom(@event.Room));
    }
}
