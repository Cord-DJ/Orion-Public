using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.Rooms;

public class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, IRole> {
    readonly RoomProvider roomProvider;

    public CreateRoleCommandHandler(RoomProvider roomProvider) {
        this.roomProvider = roomProvider;
    }

    public async Task<IRole> Handle(CreateRoleCommand request, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(request.RoomId);

        await room.EnsureHasPermissions(request.Sender, Permission.ManageRoles);
        return await room.CreateRole();
    }
}
