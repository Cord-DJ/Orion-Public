using Cord.Server.Domain;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.BufferHandlers;

public class VotedHandler : IHandler<Voted> {
    readonly IGatewaySender sender;

    public VotedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public async Task Handle(Voted notification, CancellationToken cancellationToken) {
        if (notification.Room is Room room) {
            await room.LoadCurrentSongVotes();
            await sender.Send(room, x => x.UpdateCurrentSong(room.Id, room.CurrentSong));
        }
    }
}
