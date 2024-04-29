using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class AddMemberCommandHandler : ICommandHandler<AddMemberCommand> {
    readonly RoomProvider roomProvider;

    public AddMemberCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(AddMemberCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);
        // await room.EnsureHasOnlineUser(sender);

        await room.SendInfoMessage($"{request.User.Name} joined the party");
        await room.AddMember(request.User);

        return Unit.Value;
    }
}
