using Cord.Server.Domain;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Users;

namespace Cord.Server.Application.BufferHandlers;

public class MessageDeletedHandler : IHandler<MessageDeleted> {
    readonly IGatewaySender gateway;

    public MessageDeletedHandler(IGatewaySender gateway) {
        this.gateway = gateway;
    }

    public async Task Handle(MessageDeleted @event, CancellationToken token = default) {
        if (@event.Room is Room room) {
            await room.EnsureMembersLoaded();

            await gateway.Send(
                room,
                async (gw, user) => {
                    if (!room.IsBanned(user.Id) && room.AllowedAction((User)user, Permission.ReadMessageHistory)) {
                        await gw.DeleteMessage(room.Id, @event.MessageId);
                    }
                }
            );
        }
    }
}
