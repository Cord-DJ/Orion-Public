using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class ExperienceGainedHandler : IHandler<ExperienceGained> {
    readonly IGatewaySender sender;

    public ExperienceGainedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(ExperienceGained notification, CancellationToken cancellationToken) {
        return sender.Send(notification.User, x => x.ValueNotification("exp", notification.Experience.ToString()));
    }
}
