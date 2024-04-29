using Cord.Science;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Timer = System.Timers.Timer;

namespace Cord;

public sealed class CordClient {
    internal static readonly HttpClient Http = new();
    HubConnection connection = default!;
    readonly TaskCompletionSource readyReceived = new();
    Timer? helloTimer;

    (string Email, string Password) credentials;

    readonly List<IRoom> rooms = new();
    readonly List<IUser> users = new();

    static readonly Dictionary<Endpoint, string> endpoints = new() {
        { Endpoint.Main, "https://cord.dj" },
        { Endpoint.Beta, "https://beta.cord.dj" },
        { Endpoint.Local, "http://localhost:4295" }
    };

    public LoginState LoginState { get; private set; } = LoginState.LoggedOut;
    public Endpoint Endpoint { get; }
    public IUser CurrentUser => users.First();

    public IReadOnlyCollection<IRoom> Rooms => rooms.AsReadOnly();
    public IReadOnlyCollection<IUser> Users => users.AsReadOnly();


    public CordClient(Endpoint endpoint = Endpoint.Main) {
        Endpoint = endpoint;
        Http.BaseAddress = new(endpoints[endpoint]);
    }

    public async Task LoginAsync(string email, string password) {
        credentials = (email, password);
        var data = new FormUrlEncodedContent(
            new Dictionary<string, string> {
                { "grant_type", "password" },
                { "scope", "offline_access api.cord.server" },
                { "client_id", "cord-dj-client" },
                { "username", email },
                { "password", password }
            }
        );

        var response = await Http.PostAsync($"{endpoints[Endpoint]}/connect/token", data);
        response.EnsureSuccessStatusCode();

        var token = (await response.Content.ReadAsAsync<dynamic>()).access_token;
        Http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}");

        connection = new HubConnectionBuilder()
            .WithUrl($"{endpoints[Endpoint]}/gateway?access_token={token}")
            .ConfigureLogging(
                logging => {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddConsole();
                }
            )
            .AddNewtonsoftJsonProtocol()
            .Build();

        MapPlaylistHandlers();
        MapGlobalHandlers();
        MapRoomHandlers();

        LoginState = LoginState.LoggingIn;
        await connection.StartAsync();

        LoginState = LoginState.LoggedIn;
        if (LoggedIn != null) {
            await LoggedIn.Invoke();
        }

        await readyReceived.Task;
    }

    public async Task LogoutAsync() {
        LoginState = LoginState.LoggingOut;
        await connection.StopAsync();

        LoginState = LoginState.LoggedOut;
        if (Disconnected != null) {
            await Disconnected.Invoke();
        }
    }

    public async Task Identify(IDevice device) {
        await connection.InvokeAsync("Identify", device);
    }

    public async Task EnterRoom(string link) {
        var id = await connection.InvokeAsync<ID>("EnterRoom", link);
    }

    public async Task<IRoom[]> GetPopularRooms() {
        var response = await Http.GetAsync("/api/rooms");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<RoomImpl[]>();
    }

    public async Task CreateRoom(string name) {
        var json = JsonConvert.SerializeObject(new { Name = name });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await Http.PostAsync("/api/rooms", content);

        response.EnsureSuccessStatusCode();
    }

    void SetupPingTimer(int interval) {
        if (helloTimer != null) {
            helloTimer.Enabled = false;
        }

        helloTimer = new();
        helloTimer.Interval = interval;
        helloTimer.Elapsed += async (src, e) => {
            if (!await Ping()) {
                await LogoutAsync();
                await LoginAsync(credentials.Email, credentials.Password);
            }
        };
        helloTimer.Enabled = true;
    }

    Task<bool> Ping() {
        Console.WriteLine("Sending ping");
        return connection.InvokeAsync<bool>("Ping");
    }

    void MapPlaylistHandlers() {
        connection.On<PlaylistImpl>(
            "CreatePlaylist",
            async playlist => {
                var user = (UserImpl)CurrentUser;
                if (user.playlists.Any(x => x.Id == playlist.Id)) {
                    throw new("Playlist already exists");
                }

                user.playlists.Add(playlist);
                if (PlaylistCreated != null) {
                    await PlaylistCreated.Invoke(playlist);
                }
            }
        );

        connection.On<PlaylistImpl>(
            "UpdatePlaylist",
            async playlist => {
                var user = (UserImpl)CurrentUser;

                var idx = user.playlists.FindIndex(x => x.Id == playlist.Id);
                if (idx != -1) {
                    user.playlists[idx] = playlist;
                }

                if (PlaylistUpdated != null) {
                    await PlaylistUpdated.Invoke(playlist);
                }
            }
        );

        connection.On<ID>(
            "DeletePlaylist",
            async playlistId => {
                var user = (UserImpl)CurrentUser;

                user.playlists.RemoveAll(x => x.Id == playlistId);
                if (PlaylistDeleted != null) {
                    await PlaylistDeleted.Invoke(playlistId);
                }
            }
        );
    }

    void MapGlobalHandlers() {
        connection.On<HelloImpl>(
            "Hello",
            async hello => {
                SetupPingTimer(hello.HelloInterval);
            }
        );

        connection.On<RoomImpl[], UserImpl>(
            "Ready",
            async (rooms, me) => {
                this.rooms.Clear();
                this.rooms.AddRange(rooms);

                users.Clear();
                users.Add(me);

                AddUsers(rooms.SelectMany(room => room.Members!.Select(member => member.User)));
                AddUsers(rooms.SelectMany(room => room.Members!.Select(member => member.User)));

                if (Ready != null) {
                    await Ready.Invoke();
                }

                readyReceived.SetResult();
            }
        );

        connection.On<UserImpl>(
            "UpdateUser",
            async user => {
                var userIdx = users.FindIndex(x => x.Id == user.Id);
                if (userIdx != -1) {
                    users[userIdx] = user;
                }

                if (UserUpdated != null) {
                    await UserUpdated.Invoke(user);
                }
            }
        );

        connection.On<RoomImpl>(
            "CreateRoom",
            async room => {
                if (rooms.Any(x => x.Id == room.Id)) {
                    return;
                }

                rooms.Add(room);
                if (RoomCreated != null) {
                    await RoomCreated.Invoke(room);
                }
            }
        );

        connection.On<RoomImpl>(
            "UpdateRoom",
            async room => {
                if (rooms.Find(x => x.Id == room.Id) is RoomImpl r) {
                    r.Name = room.Name;
                    r.Link = room.Link;
                    r.Description = room.Description;
                    r.categories = room.categories;
                    r.Features = room.Features;

                    r.Icon = room.Icon;
                    r.Banner = room.Banner;
                    r.Wallpaper = room.Wallpaper;

                    // TODO: fill missing
                }

                if (RoomUpdated != null) {
                    await RoomUpdated.Invoke(room);
                }
            }
        );

        connection.On<ID>(
            "DeleteRoom",
            async roomId => {
                rooms.RemoveAll(x => x.Id == roomId);

                if (RoomDeleted != null) {
                    await RoomDeleted.Invoke(roomId);
                }
            }
        );
    }

    void MapRoomHandlers() {
        connection.On<ID, OnlineUserImpl>(
            "CreateOnlineUser",
            async (roomId, onlineUser) => {
                var room = GetRoom(roomId);

                if (room.onlineUsers.Any(x => x.Id == onlineUser.Id)) {
                    return;
                    // throw new Exception("Online user already exists");
                }

                room.onlineUsers.Add(onlineUser);
                if (OnlineUserCreated != null) {
                    await OnlineUserCreated.Invoke(room, onlineUser);
                }
            }
        );

        connection.On<ID, OnlineUserImpl>(
            "UpdateOnlineUser",
            async (roomId, onlineUser) => {
                var room = GetRoom(roomId);

                var idx = room.onlineUsers.FindIndex(x => x.Id == onlineUser.Id);
                if (idx != -1) {
                    room.onlineUsers[idx] = onlineUser;
                }

                if (OnlineUserUpdated != null) {
                    await OnlineUserUpdated.Invoke(room, onlineUser);
                }
            }
        );

        connection.On<ID, ID>(
            "DeleteOnlineUser",
            async (roomId, onlineUserId) => {
                var room = GetRoom(roomId);

                room.onlineUsers.RemoveAll(x => x.Id == onlineUserId);
                if (OnlineUserDeleted != null) {
                    await OnlineUserDeleted.Invoke(room, onlineUserId);
                }
            }
        );

        connection.On<ID, CurrentSongImpl>(
            "UpdateCurrentSong",
            async (roomId, currentSong) => {
                var room = GetRoom(roomId);
                room.CurrentSong = currentSong;

                if (CurrentSongUpdated != null) {
                    await CurrentSongUpdated.Invoke(room, currentSong);
                }
            }
        );

        connection.On<ID, ID, Vote>(
            "UpdateVote",
            async (roomId, userId, vote) => {
                var room = GetRoom(roomId);

                if (room.CurrentSong != null) {
                    switch (vote) {
                        case Vote.Upvote:
                            ((CurrentSongImpl)room.CurrentSong).upvotes.Add(userId);
                            ((CurrentSongImpl)room.CurrentSong).downvotes.Remove(userId);
                            break;

                        case Vote.Steal:
                            ((CurrentSongImpl)room.CurrentSong).steals.Add(userId);
                            break;

                        case Vote.Downvote:
                            ((CurrentSongImpl)room.CurrentSong).downvotes.Add(userId);
                            ((CurrentSongImpl)room.CurrentSong).upvotes.Remove(userId);
                            break;
                    }
                }

                if (UserVoted != null) {
                    var user = room.members.First(x => x.User.Id == userId).User;
                    await UserVoted.Invoke(room, user, vote);
                }
            }
        );

        connection.On<ID, UserImpl[]>(
            "UpdateQueue",
            async (roomId, users) => {
                var room = GetRoom(roomId);
                room.queue.Clear();
                room.queue.AddRange(users);

                if (QueueChanged != null) {
                    await QueueChanged.Invoke(room, users);
                }
            }
        );

        connection.On<ID, RoleImpl>(
            "CreateRole",
            async (roomId, role) => {
                var room = GetRoom(roomId);

                if (room.roles.Any(x => x.Id == role.Id)) {
                    throw new("Role already exists");
                }

                room.roles.Add(role);
                if (RoleCreated != null) {
                    await RoleCreated.Invoke(room, role);
                }
            }
        );

        connection.On<ID, RoleImpl>(
            "UpdateRole",
            async (roomId, role) => {
                var room = GetRoom(roomId);

                var idx = room.roles.FindIndex(x => x.Id == role.Id);
                if (idx != -1) {
                    room.roles[idx] = role;
                }

                if (RoleUpdated != null) {
                    await RoleUpdated.Invoke(room, role);
                }
            }
        );

        connection.On<ID, ID>(
            "DeleteRole",
            async (roomId, roleId) => {
                var room = GetRoom(roomId);

                room.roles.RemoveAll(x => x.Id == roleId);
                if (RoleDeleted != null) {
                    await RoleDeleted.Invoke(room, roleId);
                }
            }
        );

        connection.On<ID, UserImpl>(
            "CreateBan",
            async (roomId, user) => {
                var room = GetRoom(roomId);

                if (room.banned.Any(x => x.Id == user.Id)) {
                    throw new("Banned user already exists");
                }

                room.banned.Add(user);
                if (BanCreated != null) {
                    await BanCreated.Invoke(room, user);
                }
            }
        );

        connection.On<ID, UserImpl>(
            "DeleteBan",
            async (roomId, user) => {
                var room = GetRoom(roomId);

                room.banned.RemoveAll(x => x.Id == user.Id);
                if (BanDeleted != null) {
                    await BanDeleted.Invoke(room, user);
                }
            }
        );

        connection.On<ID, UserImpl>(
            "CreateMute",
            async (roomId, user) => {
                var room = GetRoom(roomId);

                if (room.muted.Any(x => x.Id == user.Id)) {
                    throw new("Muted user already exists");
                }

                room.muted.Add(user);
                if (MuteCreated != null) {
                    await MuteCreated.Invoke(room, user);
                }
            }
        );

        connection.On<ID, UserImpl>(
            "DeleteMute",
            async (roomId, user) => {
                var room = GetRoom(roomId);

                room.muted.RemoveAll(x => x.Id == user.Id);
                if (MuteDeleted != null) {
                    await MuteDeleted.Invoke(room, user);
                }
            }
        );

        connection.On<ID, MemberImpl>(
            "CreateMember",
            async (roomId, member) => {
                var room = GetRoom(roomId);
                member.Load(room);

                if (room.members.Any(x => x.User.Id == member.User.Id)) {
                    throw new("Member already exists");
                }

                room.members.Add(member);
                AddUsers(new[] { member.User });

                if (MemberCreated != null) {
                    await MemberCreated.Invoke(room, member);
                }
            }
        );

        connection.On<ID, MemberImpl>(
            "UpdateMember",
            async (roomId, member) => {
                var room = GetRoom(roomId);
                member.Load(room);

                var idx = room.members.FindIndex(x => x.User.Id == member.User.Id);
                if (idx != -1) {
                    room.members[idx] = member;
                }

                if (MemberUpdated != null) {
                    await MemberUpdated.Invoke(room, member);
                }
            }
        );

        connection.On<ID, ID>(
            "DeleteMember",
            async (roomId, userId) => {
                var room = GetRoom(roomId);

                room.members.RemoveAll(x => x.User.Id == userId);
                if (MemberDeleted != null) {
                    await MemberDeleted.Invoke(room, userId);
                }
            }
        );

        connection.On<MessageImpl>(
            "CreateMessage",
            async message => {
                var room = GetRoom(message.RoomId);

                if (room.messages.Any(x => x.Id == message.Id)) {
                    throw new("Message already exists");
                }

                room.messages.Add(message);

                if (MessageCreated != null) {
                    await MessageCreated.Invoke(room, message);
                }
            }
        );

        connection.On<MessageImpl>(
            "UpdateMessage",
            async message => {
                var room = GetRoom(message.RoomId);

                var idx = room.messages.FindIndex(x => x.Id == message.Id);
                if (idx != -1) {
                    room.messages[idx] = message;
                }

                if (MessageUpdated != null) {
                    await MessageUpdated.Invoke(room, message);
                }
            }
        );

        connection.On<ID, ID>(
            "DeleteMessage",
            async (roomId, messageId) => {
                var room = GetRoom(roomId);

                room.messages.RemoveAll(x => x.Id == messageId);
                if (MessageDeleted != null) {
                    await MessageDeleted.Invoke(room, messageId);
                }
            }
        );

        connection.On<ID, SongPlayedImpl>(
            "CreateSongPlayed",
            async (roomId, songPlayed) => {
                var room = GetRoom(roomId);

                if (room.songHistory.Any(x => x.Id == songPlayed.Id)) {
                    throw new("Song already exists in the history");
                }

                room.songHistory.Add(songPlayed);

                if (SongPlayed != null) {
                    await SongPlayed.Invoke(room, songPlayed);
                }
            }
        );

        connection.On<string, string>(
            "ValueNotification",
            async (type, value) => {
                if (NotificationReceived != null) {
                    await NotificationReceived.Invoke(type, value);
                }
            }
        );
    }


    RoomImpl GetRoom(ID roomId) {
        return (RoomImpl)rooms.First(x => x.Id == roomId);
    }

    void AddUsers(IEnumerable<IUser> users) {
        foreach (var user in users) {
            if (this.users.All(x => x.Id != user.Id)) {
                this.users.Add(user);
            }
        }
    }

    public event Func<Task>? LoggedIn;
    public event Func<Task>? Disconnected;
    public event Func<Task>? Ready;

    public event Func<IUser, Task>? UserUpdated;
    public event Func<IRoom, ICurrentSong?, Task>? CurrentSongUpdated;
    public event Func<IRoom, IUser, Vote, Task>? UserVoted;
    public event Func<IRoom, IReadOnlyCollection<IUser>, Task>? QueueChanged;

    public event Func<IRoom, Task>? RoomCreated;
    public event Func<IRoom, Task>? RoomUpdated;
    public event Func<ID, Task>? RoomDeleted;

    public event Func<IRoom, IOnlineUser, Task>? OnlineUserCreated;
    public event Func<IRoom, IOnlineUser, Task>? OnlineUserUpdated;
    public event Func<IRoom, ID, Task>? OnlineUserDeleted;

    public event Func<IRoom, IRole, Task>? RoleCreated;
    public event Func<IRoom, IRole, Task>? RoleUpdated;
    public event Func<IRoom, ID, Task>? RoleDeleted;

    public event Func<IRoom, IUser, Task>? BanCreated;
    public event Func<IRoom, IUser, Task>? BanDeleted;
    public event Func<IRoom, IUser, Task>? MuteCreated;
    public event Func<IRoom, IUser, Task>? MuteDeleted;

    public event Func<IRoom, IMember, Task>? MemberCreated;
    public event Func<IRoom, IMember, Task>? MemberUpdated;
    public event Func<IRoom, ID, Task>? MemberDeleted;

    public event Func<IRoom, IMessage, Task>? MessageCreated;
    public event Func<IRoom, IMessage, Task>? MessageUpdated;
    public event Func<IRoom, ID, Task>? MessageDeleted;

    public event Func<IPlaylist, Task>? PlaylistCreated;
    public event Func<IPlaylist, Task>? PlaylistUpdated;
    public event Func<ID, Task>? PlaylistDeleted;
    public event Func<IRoom, ISongPlayed, Task>? SongPlayed;
    public event Func<string, string, Task>? NotificationReceived;
}

public enum Endpoint {
    Main,
    Beta,
    Local
}
