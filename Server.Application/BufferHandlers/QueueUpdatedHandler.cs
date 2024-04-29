using Cord.Server.Domain;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.BufferHandlers;

public class QueueUpdatedHandler : IHandler<QueueUpdated> {
    readonly IGatewaySender sender;

    public QueueUpdatedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public async Task Handle(QueueUpdated notification, CancellationToken cancellationToken) {
        if (notification.Room is Room room) {
            await room.EnsureQueueLoaded(true);
            await sender.Send(room, x => x.UpdateQueue(room.Id, room.Queue!));
        }
    }
}
