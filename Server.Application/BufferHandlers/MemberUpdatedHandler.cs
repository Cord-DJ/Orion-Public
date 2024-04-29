using Cord.Server.Domain;
using Cord.Server.Domain.Hub;

namespace Cord.Server.Application.BufferHandlers;

public class MemberUpdatedHandler : IHandler<MemberUpdated> {
    readonly IGatewaySender sender;

    public MemberUpdatedHandler(IGatewaySender sender) {
        this.sender = sender;
    }

    public Task Handle(MemberUpdated notification, CancellationToken cancellationToken) {
        return sender.Send(notification.Room, x => x.UpdateMember(notification.Room.Id, notification.Member));
    }
}
