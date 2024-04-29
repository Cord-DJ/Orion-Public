using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class RemoveMemberRoleCommandHandler : ICommandHandler<RemoveMemberRoleCommand> {
    readonly RoomProvider roomProvider;

    public RemoveMemberRoleCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(RemoveMemberRoleCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);

        await room.EnsureHasPermissions(request.Sender, Permission.ManageRoles);
        await room.EnsureCanEditRole(request.Sender, room.GetRole(request.RoleId));

        var member = await room.GetMember(request.User.Id);
        await member.RemoveRole(request.RoleId);

        return Unit.Value;
    }
}
