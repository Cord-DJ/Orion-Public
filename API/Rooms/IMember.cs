namespace Cord;

public interface IMember {
    IUser User { get; }
    string? Nick { get; }
    string? Avatar { get; }

    IReadOnlyCollection<ID> Roles { get; }

    DateTimeOffset JoinedAt { get; }
    DateTimeOffset? BoostingSince { get; }

    Permission Permissions { get; }
    // IAvatarInfo? AvatarInfo { get; }

    Task AssignRoles(ID[] roleIds);
    Task AddRole(ID roleId);
    Task RemoveRole(ID roleId);
}
