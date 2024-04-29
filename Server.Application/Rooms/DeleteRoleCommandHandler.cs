using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand> {
    readonly RoomProvider roomProvider;

    public DeleteRoleCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);

        await room.EnsureHasPermissions(request.Sender, Permission.ManageRoles);
        await room.EnsureCanEditRole(request.Sender, room.GetRole(request.RoleId));

        if (await room.DeleteRole(request.RoleId)) {
            return Unit.Value;
        }

        throw new NotFoundException(nameof(IRole), request.RoleId);
    }
}
