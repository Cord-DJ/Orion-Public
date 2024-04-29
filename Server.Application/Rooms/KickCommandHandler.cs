using Cord.Server.Application.Users;
using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class KickCommandHandler : ICommandHandler<KickCommand> {
    readonly UserProvider userProvider;
    readonly RoomProvider roomProvider;

    public KickCommandHandler(UserProvider userProvider, RoomProvider roomProvider) {
        this.userProvider = userProvider;
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(KickCommand request, CancellationToken cancellationToken) {
        if (request.Sender.Id == request.ReceiverId) {
            throw new NotAllowedException();
        }

        var kicked = await userProvider.GetUser(request.ReceiverId);
        var room = await roomProvider.GetRoom(request.RoomId);
        await room.EnsureHasPermissions(request.Sender, Permission.Kick, kicked);

        await room.Kick(kicked);
        return Unit.Value;
    }
}
