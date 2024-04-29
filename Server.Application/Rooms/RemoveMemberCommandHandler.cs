using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class RemoveMemberCommandHandler : ICommandHandler<RemoveMemberCommand> {
    readonly RoomProvider roomProvider;

    public RemoveMemberCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(RemoveMemberCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);

        if (room.OwnerId == request.User.Id) {
            throw new NotAllowedException();
        }

        await room.SendInfoMessage($"{request.User.Name} left the party");
        await room.RemoveMember(request.User);

        return Unit.Value;
    }
}
