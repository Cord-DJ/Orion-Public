using Cord.Server.Domain;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Users;

namespace Cord.Server.Application.BufferHandlers;

public class MessageCreatedHandler : IHandler<MessageCreated> {
    readonly IGatewaySender gateway;

    public MessageCreatedHandler(IGatewaySender gateway) {
        this.gateway = gateway;
    }

    public async Task Handle(MessageCreated @event, CancellationToken token = default) {
        if (@event.Room is Room room) {
            await room.EnsureMembersLoaded();

            await gateway.Send(
                room,
                async (gw, user) => {
                    if (!room.IsBanned(user.Id) && user is User u && room.AllowedAction(u, Permission.ReadMessageHistory)) {
                        await gw.CreateMessage(@event.Message);
                    }
                }
            );
        }
    }
}
