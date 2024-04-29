using Cord.Server.Application.Users;
using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class MuteCommandHandler : ICommandHandler<MuteCommand> {
    readonly UserProvider userProvider;
    readonly RoomProvider roomProvider;

    public MuteCommandHandler(UserProvider userProvider, RoomProvider roomProvider) {
        this.userProvider = userProvider;
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(MuteCommand request, CancellationToken cancellationToken) {
        if (request.Sender.Id == request.ReceiverId) {
            throw new NotAllowedException();
        }

        var muted = await userProvider.GetUser(request.ReceiverId);
        var room = await roomProvider.GetRoom(request.RoomId);
        await room.EnsureHasPermissions(request.Sender, Permission.Mute, muted);

        await room.Mute(muted, request.Duration != null ? TimeSpan.FromMinutes(request.Duration.Value) : null);
        await room.SendInfoMessage($"{muted.Name} has been muted by {request.Sender.Name}!");

        return Unit.Value;
    }
}
