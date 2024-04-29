using Cord.Server.Domain;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.BufferHandlers;

public class MemberAddedHandler : IHandler<MemberAdded> {
    readonly IGatewaySender gateway;

    public MemberAddedHandler(IGatewaySender gateway) {
        this.gateway = gateway;
    }

    public async Task Handle(MemberAdded @event, CancellationToken token = default) {
        var member = @event.Member;

        if (@event.Room is Room room) {
            // TODO: use for every visible user?
            await gateway.Send(room, x => x.CreateMember(room.Id, member));

            await room.EnsureMessagesLoaded();
            foreach (var msg in room.Messages!) {
                await gateway.Send(member.User, x => x.CreateMessage(msg));
            }
        }
    }
}
