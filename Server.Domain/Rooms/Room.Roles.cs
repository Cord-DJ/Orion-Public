using Cord.Server.Domain.Users;

namespace Cord.Server.Domain.Rooms;

public sealed partial class Room {
    public IRole GetRole(ID roleId) {
        var role = Roles.FirstOrDefault(x => x.Id == roleId);
        if (role == null) {
            throw new NotFoundException(nameof(IRole), roleId);
        }

        return role;
    }

    public IRole GetMemberPrimaryRole(IMember member) {
        var roles = GetUserRoles(member.User.Id);
        if (roles.Length == 0) {
            throw new("member doesn't have roles");
        }

        return roles.MaxBy(x => x.Position)!;
    }

    public Permission GetUserPermissions(ID userId) {
        if (OwnerId == userId) {
            return Permission.All;
        }

        var roles = GetUserRoles(userId);
        return roles.Aggregate(Permission.None, (current, role) => current | role.Permissions);
    }

    public async Task EnsureCanEditRole(User user, IRole role) {
        if (user.Id == OwnerId || user.HasProperties(UserProperties.Staff)) {
            return;
        }

        var senderMember = await GetMember(user.Id);
        var senderRole = GetMemberPrimaryRole(senderMember);
        if (role.Position >= senderRole.Position) {
            throw new NotAllowedException();
        }
    }

    public bool AllowedAction(User actor, Permission permission, User? affected = null) {
        if (OwnerId == actor.Id) {
            return true;
        }

        if (affected != null && affected.HasProperties(UserProperties.Staff) && affected.Id != actor.Id) {
            return false;
        }

        if (actor.HasProperties(UserProperties.Staff)) {
            return true;
        }

        if (affected != null && OwnerId == affected.Id) {
            return false;
        }

        var actorRoles = GetUserRoles(actor.Id);
        var contains = actorRoles.Any(
            role =>
                (role.Permissions & permission) == permission
                || (role.Permissions & Permission.Administrator) == Permission.Administrator
        );

        if (!contains) {
            return false;
        }

        if (affected == null) {
            return true;
        }

        static int HighestPos(IEnumerable<IRole> roles) {
            return roles.Select(x => x.Position).Max();
        }

        var affectedRoles = GetUserRoles(affected.Id);
        return HighestPos(actorRoles) > HighestPos(affectedRoles);
    }

    public async Task EnsureHasPermissions(User actor, Permission permissions, User? affected = null) {
        await EnsureMembersLoaded();

        if (IsBanned(actor.Id)) {
            throw new UnauthorizedException();
        }

        if (!AllowedAction(actor, permissions, affected)) {
            throw new PermissionsException(permissions);
        }
    }

    public async Task<IRole> CreateRole() {
        var role = new Role(ID.NewId(), 1, "new role", "#424242", RoleSettings.Default, Permission.Everyone);

        // Shift position in roles
        var newRoles = Roles.Select(
                x => {
                    if (x.Id != Role.EveryoneId) {
                        return (Role)x with { Position = x.Position + 1 };
                    }

                    return x;
                }
            )
            .ToList();

        newRoles.Add(role);
        SanityRoles(model.Roles);

        model.Roles.Clear();
        model.Roles.AddRange(newRoles);

        await Save();
        await DomainEvents.Raise(new RoleCreated(this, role));

        return role;
    }

    public async Task<bool> DeleteRole(ID roleId) {
        if (roleId == Role.EveryoneId) {
            return false;
        }

        model.Roles.RemoveAll(x => x.Id == roleId);

        await EnsureMembersLoaded();
        foreach (var member in Members!) {
            await member.RemoveRole(roleId);
        }

        await Save();
        await DomainEvents.Raise(new RoleDeleted(this, roleId));
        return true;
    }

    public async Task ReorderRoles(ID[] order) {
        if (order.Length != Roles.Count) {
            return;
        }

        foreach (var (id, i) in order.Select((v, i) => (v, i))) {
            if (i == 0) {
                continue;
            }

            var roleIdx = model.Roles.FindIndex(x => x.Id == id);
            if (roleIdx != -1 && id != Role.EveryoneId) {
                model.Roles[roleIdx] = (Role)model.Roles[roleIdx] with { Position = i };
            }
        }

        await Save();

        foreach (var role in Roles) {
            await DomainEvents.Raise(new RoleUpdated(this, role));
        }
    }

    public async Task<IRole> UpdateRole(IRole role, UpdateRole values) {
        // TODO: refactor this
        var idx = model.Roles.FindIndex(x => x.Id == role.Id);
        if (idx == -1) {
            throw new NotFoundException(nameof(Role), role.Id);
        }

        model.Roles[idx] = (Role)model.Roles[idx] with {
            Name = role.Id == Role.EveryoneId ? model.Roles[idx].Name : values.Name,
            Color = values.Color,
            Permissions = values.Permissions
        };

        await Save();
        await DomainEvents.Raise(new RoleUpdated(this, model.Roles[idx]));
        return model.Roles[idx];
    }

    IRole[] GetUserRoles(ID userId) {
        if (members == null) {
            throw new("members not loaded");
        }

        var roles = members.Find(x => x.User.Id == userId)?.Roles;
        return roles == null ? Array.Empty<IRole>() : Roles.Where(x => roles.Contains(x.Id)).ToArray();
    }

    void SanityRoles(List<IRole> roles) {
        roles.Sort((a, b) => a.Position - b.Position);

        foreach (var (role, i) in roles.Select((v, i) => (v, i))) {
            ((Role)role).Position = i;
        }
    }
}
