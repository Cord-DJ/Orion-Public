using Cord.Server.Domain.Rooms;
using MediatR;

namespace Cord.Server.Application.Rooms;

public class AddMemberRoleCommandHandler : ICommandHandler<AddMemberRoleCommand> {
    readonly RoomProvider roomProvider;

    public AddMemberRoleCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<Unit> Handle(AddMemberRoleCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);

        await room.EnsureHasPermissions(request.Sender, Permission.ManageRoles);
        await room.EnsureCanEditRole(request.Sender, room.GetRole(request.RoleId));

        var member = await room.GetMember(request.User.Id);
        await member.AddRole(request.RoleId);

        return Unit.Value;
    }
}
