using Cord.Server.Application.Users;
using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class PresetChangedHandler : IHandler<PresetChanged> {
    readonly IGatewaySender sender;
    readonly UserProvider userProvider;

    public PresetChangedHandler(IGatewaySender sender, UserProvider userProvider) {
        this.sender = sender;
        this.userProvider = userProvider;
    }

    public async Task Handle(PresetChanged @event, CancellationToken cancellationToken) {
        if (@event.IsPrimary) {
            await userProvider.LoadCharacter(@event.User);
            await sender.SendToAllVisibleUsersOfUser(@event.User, x => x.UpdateUser(@event.User));
        }
    }
}
