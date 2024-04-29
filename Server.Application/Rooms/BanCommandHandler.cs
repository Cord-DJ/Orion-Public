using Cord.Server.Application.Users;
using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class BanCommandHandler : ICommandHandler<BanCommand> {
    readonly UserProvider userProvider;
    readonly RoomProvider roomProvider;

    public BanCommandHandler(UserProvider userProvider, RoomProvider roomProvider) {
        this.userProvider = userProvider;
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(BanCommand request, CancellationToken cancellationToken) {
        if (request.Sender.Id == request.ReceiverId) {
            throw new NotAllowedException();
        }

        var banned = await userProvider.GetUser(request.ReceiverId);
        var room = await roomProvider.GetRoom(request.RoomId);
        await room.EnsureHasPermissions(request.Sender, Permission.Ban, banned);

        await room.Ban(banned, request.Duration != null ? TimeSpan.FromMinutes(request.Duration.Value) : null);
        await room.SendInfoMessage($"{banned.Name} has been banned by {request.Sender.Name}!");

        return Unit.Value;
    }
}
