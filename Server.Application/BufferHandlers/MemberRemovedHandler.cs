using Cord.Server.Domain;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.BufferHandlers;

public class MemberRemovedHandler : IHandler<MemberRemoved> {
    readonly IGatewaySender gateway;

    public MemberRemovedHandler(IGatewaySender gateway) {
        this.gateway = gateway;
    }

    public async Task Handle(MemberRemoved @event, CancellationToken token = default) {
        var user = @event.Member.User;

        if (@event.Room is Room room) {
            await gateway.Send(room, x => x.DeleteMember(room.Id, user.Id));
            await gateway.Send(user, x => x.DeleteMember(room.Id, user.Id));
        }
    }
}
