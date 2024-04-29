using Cord.Server.Domain;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.BufferHandlers;

public class ActiveRoomSetHandler : IHandler<ActiveRoomSet> {
    readonly IGatewaySender sender;

    public ActiveRoomSetHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public async Task Handle(ActiveRoomSet @event, CancellationToken cancellationToken) {
        if (@event.Room is Room room) {
            await sender.Send(@event.User, x => x.CreateRoom(room));

            await sender.Send(
                room,
                gateway => {
                    var userId = @event.User.Id;
                    var onlineUser = room.OnlineUsers!.First(x => x.Id == userId);

                    if (@event.Exists) {
                        return gateway.UpdateOnlineUser(room.Id, onlineUser);
                    }

                    return gateway.CreateOnlineUser(room.Id, onlineUser);
                }
            );
        }
    }
}
