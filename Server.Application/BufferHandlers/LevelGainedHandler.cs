using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class LevelGainedHandler : IHandler<LevelGained> {
    readonly IGatewaySender sender;

    public LevelGainedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(LevelGained notification, CancellationToken cancellationToken) {
        return sender.Send(notification.User, x => x.ValueNotification("level", notification.Level.ToString()));
    }
}
