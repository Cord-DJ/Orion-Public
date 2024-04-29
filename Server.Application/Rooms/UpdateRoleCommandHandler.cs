using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.Rooms;

public class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, IRole> {
    readonly RoomProvider roomProvider;

    public UpdateRoleCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<IRole> Handle(UpdateRoleCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);

        await room.EnsureHasPermissions(request.Sender, Permission.ManageRoles);
        return await room.UpdateRole(room.Roles.First(x => x.Id == request.RoleId), request.UpdateRole);
    }
}
