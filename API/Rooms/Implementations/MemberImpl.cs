using Newtonsoft.Json;

namespace Cord;

sealed class MemberImpl : IMember {
    readonly List<ID> roles;
    IRoom? room;

    public IUser User { get; init; }
    public string? Nick { get; init; }
    public string? Avatar { get; init; }
    public IReadOnlyCollection<ID> Roles => roles.AsReadOnly();

    public DateTimeOffset JoinedAt { get; init; }
    public DateTimeOffset? BoostingSince { get; init; }

    public Permission Permissions { get; init; }

    [JsonConstructor]
    public MemberImpl(
        UserImpl user,
        string? nick,
        string? avatar,
        List<ID> roles,
        DateTimeOffset joinedAt,
        DateTimeOffset? boostingSince,
        Permission permissions
    ) {
        User = user;
        Nick = nick;
        Avatar = avatar;
        this.roles = roles;

        JoinedAt = joinedAt;
        BoostingSince = boostingSince;

        Permissions = permissions;
    }

    public void Load(IRoom room) {
        this.room = room;
    }

    public async Task AssignRoles(ID[] roleIds) => throw new NotImplementedException();

    public async Task AddRole(ID roleId) {
        var response = await CordClient.Http.PostAsync($"/api/rooms/{room?.Id}/members/{User.Id}/role/{roleId}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveRole(ID roleId) {
        var response = await CordClient.Http.DeleteAsync($"/api/rooms/{room?.Id}/members/{User.Id}/role/{roleId}");
        response.EnsureSuccessStatusCode();
    }

    public override bool Equals(object? obj) => obj is IMember entity && User.Id == entity.User.Id;
    public override int GetHashCode() => User.Id.GetHashCode();
}
