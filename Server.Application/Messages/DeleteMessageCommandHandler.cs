using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Users;
using MediatR;

namespace Cord.Server.Application.Messages;

public class DeleteMessageCommandHandler : ICommandHandler<DeleteMessageCommand> {
    readonly RoomProvider roomProvider;

    public DeleteMessageCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(DeleteMessageCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);

        await room.EnsureMessagesLoaded();
        var message = room.Messages!.First(x => x.Id == request.MessageId);
        var user = message.Member?.User ?? message.Author;

        await room.EnsureHasPermissions(request.Sender, Permission.ManageMessages, user as User);
        await room.DeleteMessage(message);

        return Unit.Value;
    }
}
