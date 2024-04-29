using Newtonsoft.Json;
using System.Text;

namespace Cord;

sealed class RoomImpl : IRoom, ISnowflakeEntity {
    internal readonly List<IOnlineUser> onlineUsers = new();
    internal readonly List<IMember> members = new();
    internal readonly List<IUser> queue = new();
    internal readonly List<IRole> roles = new();

    internal readonly List<IUser> banned = new();
    internal readonly List<IUser> muted = new();
    internal readonly List<IMessage> messages = new();

    internal readonly List<ISongPlayed> songHistory = new();
    internal List<Category> categories = new();

    public ID Id { get; }
    public ID OwnerId { get; }
    public string Name { get; internal set; }
    public string Link { get; internal set; }
    public string? Description { get; internal set; }
    public IReadOnlyCollection<Category> Categories => categories.AsReadOnly();
    public RoomFeature Features { get; internal set; }

    public string? Icon { get; internal set; }
    public string? Banner { get; internal set; }
    public string? Wallpaper { get; internal set; }

    public long? MemberCount { get; }
    public long? OnlineCount { get; }

    public ICurrentSong? CurrentSong { get; internal set; }

    public IReadOnlyCollection<IOnlineUser>? OnlineUsers => onlineUsers.AsReadOnly();
    public IReadOnlyCollection<IMember>? Members => members.AsReadOnly();
    public IReadOnlyCollection<IUser>? Queue => queue.AsReadOnly();
    public IReadOnlyCollection<IRole> Roles => roles.AsReadOnly();

    public IReadOnlyCollection<IUser>? Banned => banned.AsReadOnly();
    public IReadOnlyCollection<IUser>? Muted => muted.AsReadOnly();

    public IReadOnlyCollection<IMessage>? Messages => messages.AsReadOnly();

    public IReadOnlyCollection<ISongPlayed>? SongHistory => songHistory.AsReadOnly();

    [JsonConstructor]
    internal RoomImpl(
        ID id,
        ID ownerId,
        string name,
        string link,
        string? description,
        Category[] categories,
        RoomFeature features,
        string? icon,
        string? banner,
        string? wallpaper,
        long? memberCount,
        long? onlineCount,
        CurrentSongImpl? currentSong,
        OnlineUserImpl[]? onlineUsers,
        MemberImpl[]? members,
        UserImpl[]? queue,
        RoleImpl[] roles,
        UserImpl[]? banned,
        UserImpl[]? muted,
        MessageImpl[]? messages,
        SongPlayedImpl[]? songHistory
    ) {
        Id = id;
        OwnerId = ownerId;
        Name = name;
        Link = link;
        Description = description;
        Features = features;

        Icon = icon;
        Banner = banner;
        Wallpaper = wallpaper;

        MemberCount = memberCount;
        OnlineCount = onlineCount;

        CurrentSong = currentSong;

        this.onlineUsers.AddRange(onlineUsers ?? Array.Empty<IOnlineUser>());
        this.members.AddRange(members ?? Array.Empty<IMember>());
        this.queue.AddRange(queue ?? Array.Empty<IUser>());
        this.roles.AddRange(roles);

        this.messages.AddRange(messages ?? Array.Empty<IMessage>());
        this.songHistory.AddRange(songHistory ?? Array.Empty<ISongPlayed>());
        this.categories.AddRange(categories);
        this.banned.AddRange(banned ?? Array.Empty<IUser>());
        this.muted.AddRange(muted ?? Array.Empty<IUser>());
    }

    public async Task Vote(Vote vote) {
        var response = await CordClient.Http.PostAsync($"/api/rooms/{Id}/vote/{vote.ToString().ToLower()}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task Update(UpdateRoom room) {
        var json = JsonConvert.SerializeObject(room);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PatchAsync($"/api/rooms/{Id}", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task Delete() {
        var response = await CordClient.Http.DeleteAsync($"/api/rooms/{Id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task Kick(IUser user) {
        var response = await CordClient.Http.PostAsync($"/api/rooms/{Id}/users/{user.Id}/kick", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task Ban(IUser user, TimeSpan? duration) {
        var json = JsonConvert.SerializeObject(new { Duration = duration?.TotalMinutes });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PostAsync($"/api/rooms/{Id}/users/{user.Id}/ban", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task Mute(IUser user, TimeSpan? duration) {
        var json = JsonConvert.SerializeObject(new { Duration = duration?.TotalMinutes });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PostAsync($"/api/rooms/{Id}/users/{user.Id}/mute", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task Unban(IUser user) {
        var response = await CordClient.Http.DeleteAsync($"/api/rooms/{Id}/users/{user.Id}/ban");
        response.EnsureSuccessStatusCode();
    }

    public async Task Unmute(IUser user) {
        var response = await CordClient.Http.DeleteAsync($"/api/rooms/{Id}/users/{user.Id}/mute");
        response.EnsureSuccessStatusCode();
    }

    public async Task<IRole> CreateRole() {
        var response = await CordClient.Http.PostAsync($"/api/rooms/{Id}/roles", null);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<RoleImpl>();
    }

    public async Task DeleteRole(IRole role) {
        var response = await CordClient.Http.DeleteAsync($"/api/rooms/{Id}/roles/{role.Id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<IRole> UpdateRole(IRole role, UpdateRole values) {
        var json = JsonConvert.SerializeObject(values);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PatchAsync($"/api/rooms/{Id}/roles/{role.Id}", content);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<RoleImpl>();
    }

    public async Task ReorderRoles(IRole[] roles) {
        var json = JsonConvert.SerializeObject(roles.Select(x => x.Id));
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PatchAsync($"/api/rooms/{Id}/roles", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task<IMessage> SendMessage(string message) {
        var json = JsonConvert.SerializeObject(new { Message = message });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PostAsync($"/api/rooms/{Id}/messages", content);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<MessageImpl>();
    }

    public async Task ReorderQueue(ID[] ids) {
        var json = JsonConvert.SerializeObject(ids);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PatchAsync($"/api/rooms/{Id}/queue", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task AddToQueue(IUser user) {
        var response = await CordClient.Http.PutAsync($"/api/rooms/{Id}/members/@me/queue", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveFromQueue(IUser user) {
        var response = await CordClient.Http.DeleteAsync($"/api/rooms/{Id}/members/{user.Id}/queue");
        response.EnsureSuccessStatusCode();
    }

    public async Task AddMember(IUser user) {
        var response = await CordClient.Http.PostAsync($"/api/rooms/{Id}/members/{user.Id}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveMember(IUser user) {
        var response = await CordClient.Http.DeleteAsync($"/api/rooms/{Id}/members/{user.Id}");
        response.EnsureSuccessStatusCode();
    }

    public override bool Equals(object? obj) => obj is IRoom entity && Id == entity.Id;
    public override int GetHashCode() => Id.GetHashCode();
}
