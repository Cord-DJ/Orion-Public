using Cord.Server.Application.Users;
using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class UnmuteCommandHandler : ICommandHandler<UnmuteCommand> {
    readonly UserProvider userProvider;
    readonly RoomProvider roomProvider;

    public UnmuteCommandHandler(UserProvider userProvider, RoomProvider roomProvider) {
        this.userProvider = userProvider;
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(UnmuteCommand request, CancellationToken cancellationToken) {
        var muted = await userProvider.GetUser(request.ReceiverId);
        var room = await roomProvider.GetRoom(request.RoomId);
        await room.EnsureHasPermissions(request.Sender, Permission.Mute, muted);

        await room.Unmute(muted);
        return Unit.Value;
    }
}
