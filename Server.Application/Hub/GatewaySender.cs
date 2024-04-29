using Cord.Server.Application.Users;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Users;
using Microsoft.AspNetCore.SignalR;

namespace Cord.Server.Application.Hub;

public sealed class GatewaySender : IGatewaySender {
    readonly IHubContext<GatewayHub, IGatewayAPI> hub;
    readonly UserProvider userProvider;

    public GatewaySender(IHubContext<GatewayHub, IGatewayAPI> hub, UserProvider userProvider) {
        this.hub = hub;
        this.userProvider = userProvider;
    }

    public Task Send(IUser user, Func<IGatewayAPI, Task> callback) => callback(GatewayForUser(user));

    public Task Send(IRoom room, Func<IGatewayAPI, Task> callback) {
        return Send(room, (api, _) => callback(api));
    }

    public Task Send(IRoom room, Func<IGatewayAPI, IUser, Task> callback) {
        var sentIds = new List<ID>();
        return InternalRoomSend(room, callback, sentIds);
    }

    public Task SendToAllVisibleUsersOfUser(IUser user, Func<IGatewayAPI, Task> callback) {
        return SendToAllVisibleUsersOfUser(user, (api, _) => callback(api));
    }

    public async Task SendToAllVisibleUsersOfUser(IUser user, Func<IGatewayAPI, IUser, Task> callback) {
        var sentIds = new List<ID> { user.Id };

        // Send at least to self if no rooms are joined
        await callback(GatewayForUser(user), user);

        var tasks = await userProvider
            .GetJoinedRooms((User)user)
            .Select(room => InternalRoomSend(room, callback, sentIds))
            .ToListAsync();

        await Task.WhenAll(tasks);
    }

    IGatewayAPI GatewayForUser(IUser user) => hub.Clients.Group($"user_{user.Id}");

    async Task InternalRoomSend(IRoom room, Func<IGatewayAPI, IUser, Task> callback, List<ID> sentIds) {
        if (room is not Room r) {
            throw new NotSupportedException();
        }

        await r.EnsureOnlineUsersLoaded(); // OnlineUsers and Members
        var tasks = new List<Task>();

        foreach (var member in r.Members!) {
            var user = (User)member.User;

            if (!sentIds.Contains(user.Id)) {
                sentIds.Add(user.Id);
                tasks.Add(callback(GatewayForUser(user), user));
            }
        }

        foreach (var online in r.OnlineUsers!) {
            if (online.User != null && !sentIds.Contains(online.User.Id)) {
                tasks.Add(callback(GatewayForUser(online.User), online.User));
            }
        }

        await Task.WhenAll(tasks);
    }
}
