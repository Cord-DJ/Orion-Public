using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class CurrentSongUpdatedHandler : IHandler<CurrentSongUpdated> {
    readonly IGatewaySender sender;

    public CurrentSongUpdatedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(CurrentSongUpdated notification, CancellationToken cancellationToken) {
        return sender.Send(notification.Room, x => x.UpdateCurrentSong(notification.Room.Id, notification.NewSong));
    }
}
