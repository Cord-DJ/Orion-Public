using App.Metrics;
using Cord.Science;
using Cord.Server.Application.Users;
using Cord.Server.Domain;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Science;
using Cord.Server.Domain.Users;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace Cord.Server.Application.Hub;

public sealed class GatewayHub : Hub<IGatewayAPI> {
    readonly IMetrics metrics;
    readonly RoomProvider roomProvider;
    readonly UserProvider userProvider;
    readonly IGuestRepository guestRepository;
    readonly IDeviceInfoRepository deviceInfoRepository;
    readonly IGatewaySender gatewaySender;

    public GatewayHub(
        IMetrics metrics,
        RoomProvider roomProvider,
        UserProvider userProvider,
        IGuestRepository guestRepository,
        IDeviceInfoRepository deviceInfoRepository,
        IGatewaySender gatewaySender
    ) {
        this.metrics = metrics;
        this.roomProvider = roomProvider;
        this.userProvider = userProvider;
        this.guestRepository = guestRepository;
        this.deviceInfoRepository = deviceInfoRepository;
        this.gatewaySender = gatewaySender;
    }

    public override async Task OnConnectedAsync() {
        metrics.Measure.Counter.Increment(MetricRegistry.OpenSessions);

        User? user = null;
        if (ID.TryParse(Context.UserIdentifier, out var userId)) {
            try {
                user = await userProvider.GetUser(userId);
            } catch (NotFoundException) { }
        }

        if (user == null) {
            metrics.Measure.Counter.Increment(MetricRegistry.GuestsCreated);

            user = await userProvider.CreateGuestUser();
            await guestRepository.Add(new(Context.ConnectionId, user.Id));
        }

        await user.LogLoggedInTimer();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{user.Id}");

        await gatewaySender.Send(user, x => x.Hello(new Hello(45000)));
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? e) {
        metrics.Measure.Counter.Decrement(MetricRegistry.OpenSessions);

        try {
            var sender = await Sender();
            if (sender.IsGuest) {
                Log.Information("Disconnecting guest {Name}", sender.Name);
                await sender.RemoveActiveRooms();
                await guestRepository.Remove(Context.ConnectionId);
            }
        } catch { } // Not logged in

        await base.OnDisconnectedAsync(e);
    }

    public async Task Identify(Device device) {
        var sender = await Sender();
        var headers = Context.GetHttpContext()?.Request.Headers;
        var feature = Context.Features.Get<IHttpConnectionFeature>();

        await deviceInfoRepository.Add(
            new(
                ID.NewId(),
                sender.Id,
                DeviceAction.Identify,
                device,
                headers?["User-Agent"],
                feature?.RemoteIpAddress?.ToString()
            )
        );

        await SendReady(sender);
    }

    public async Task<ID?> EnterRoom(string roomLink) {
        var sender = await Sender();
        var room = await roomProvider.GetRoomByLink(roomLink);

        if (room == null || room.IsBanned(sender.Id)) {
            return null;
        }

        // TODO: allow entering multiple rooms for bots
        // We should let users browse other rooms and not get remove from the main one
        // when they are in queue, etc.
        // try {
        // await sender.RemoveActiveRooms();
        // } catch (NotFoundException) { }

        Log.Information("User {Name} is entering room {Room}", sender.Name, room.Name);
        await sender.SetActiveRoom(room);

        metrics.Measure.Counter.Increment(MetricRegistry.RoomEnters);
        return room.Id;
    }

    public async Task<bool> Ping() {
        var sender = await Sender();
        var roomCount = await sender.Ping();

        if (roomCount < 1) {
            Log.Error("User {Name} is not in any room but pinged", sender.Name);
            return false;
        }

        Log.Information("User {Name} pinged", sender.Name);
        return true;
    }

    async Task<User> Sender() {
        var id = Context.UserIdentifier != null
            ? ID.Parse(Context.UserIdentifier)
            : (await guestRepository.Get(Context.ConnectionId))?.UserId;
        if (id == null) {
            throw new UnauthorizedException();
        }

        return await userProvider.GetUser(id.Value);
    }

    async Task SendReady(User user) {
        await user.EnsurePlaylistsLoaded();
        await userProvider.LoadCharacter(user);

        var rooms = await userProvider.GetJoinedRooms(user).ToListAsync();
        await Task.WhenAll(rooms.Select(x => x.LoadAll(user)));

        user.SetPrivateFields();
        await gatewaySender.Send(user, x => x.Ready(rooms, user));
    }
}

sealed record Hello(int HelloInterval) : IHello;

// var token = await identityServerTools.IssueClientJwtAsync("cord-dj-client", 400, null, new List<string> { "api.cord.server" }, new List<Claim> {
//     new Claim("sub", user.Id.ToString()),
//     new Claim("role", "guest")
// });
// await user.Context.SetToken(token);
