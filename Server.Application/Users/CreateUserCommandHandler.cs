using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.Users;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, IUser> {
    readonly UserProvider userProvider;
    readonly RoomProvider roomProvider;

    public CreateUserCommandHandler(UserProvider userProvider, RoomProvider roomProvider) {
        this.userProvider = userProvider;
        this.roomProvider = roomProvider;
    }

    public async Task<IUser> Handle(CreateUserCommand request, CancellationToken cancellationToken) {
        if (await userProvider.ExistEmail(request.Email)) {
            throw new ConflictException(nameof(request.Email));
        }

        var user = await userProvider.CreateUser(request.Email, request.Password, request.Name);
        var room = await roomProvider.GetRoomByLink("home");
        await room!.AddMember(user);

        return user;
    }
}
