using Cord.Server.Application.Users;
using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class UnbanCommandHandler : ICommandHandler<UnbanCommand> {
    readonly UserProvider userProvider;
    readonly RoomProvider roomProvider;

    public UnbanCommandHandler(UserProvider userProvider, RoomProvider roomProvider) {
        this.userProvider = userProvider;
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(UnbanCommand request, CancellationToken cancellationToken) {
        var banned = await userProvider.GetUser(request.ReceiverId);
        var room = await roomProvider.GetRoom(request.RoomId);
        await room.EnsureHasPermissions(request.Sender, Permission.Ban, banned);

        await room.Unban(banned);
        return Unit.Value;
    }
}
