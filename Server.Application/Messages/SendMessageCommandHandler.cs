using App.Metrics;
using Cord.Server.Domain;
using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.Messages;

public class SendMessageCommandHandler : ICommandHandler<SendMessageCommand, IMessage> {
    readonly RoomProvider roomProvider;
    readonly IMetrics metrics;

    public SendMessageCommandHandler(RoomProvider roomProvider, IMetrics metrics) {
        this.roomProvider = roomProvider;
        this.metrics = metrics;
    }

    public async Task<IMessage> Handle(SendMessageCommand notification, CancellationToken cancellationToken) {
        var room = await roomProvider.GetRoom(notification.RoomId);
        await room.EnsureHasOnlineUser(notification.Sender);
        await room.EnsureHasPermissions(notification.Sender, Permission.SendMessages);

        if (room.IsMuted(notification.Sender.Id)) {
            throw new PermissionsException(Permission.SendMessages);
        }

        var member = await room.GetMember(notification.Sender.Id);
        var msg = await room.SendMessage(member, notification.Message);
        metrics.Measure.Counter.Increment(MetricRegistry.Messages);

        return msg;
    }
}
