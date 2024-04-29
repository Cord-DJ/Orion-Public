namespace Cord.Server.Domain.Rooms;

public sealed class Member : IMember {
    readonly IMemberRepository memberRepository;

    Room room = default!;
    MemberModel model = default!;

    public ID Id => model.Id;

    public string? Nick => model.Nick;
    public string? Avatar => model.Avatar;
    public IReadOnlyCollection<ID> Roles => model.Roles.AsReadOnly();

    public DateTimeOffset JoinedAt => model.JoinedAt;
    public DateTimeOffset? BoostingSince => model.BoostingSince;

    public IUser User { get; private set; } = default!;
    public Permission Permissions => room.GetUserPermissions(model.UserId);


    public Member(IMemberRepository memberRepository) {
        this.memberRepository = memberRepository;
    }

    public void Load(MemberModel model, Room room, IUser user) {
        this.model = model;
        this.room = room;
        User = user;
    }

    public void ClearRoles() {
        model.Roles.Clear();
    }

    public Task Save() => memberRepository.Update(model);

    public async Task AssignRoles(ID[] roleIds) {
        model.Roles.Clear();
        model.Roles.AddRange(roleIds);

        await Save();
        await DomainEvents.Raise(new MemberUpdated(room, this));
    }

    public async Task AddRole(ID role) {
        if (model.Roles.Contains(role)) {
            return;
        }

        model.Roles.Add(role);

        await Save();
        await DomainEvents.Raise(new MemberUpdated(room, this));
    }

    public async Task RemoveRole(ID role) {
        if (!model.Roles.Contains(role)) {
            return;
        }

        model.Roles.Remove(role);

        await Save();
        await DomainEvents.Raise(new MemberUpdated(room, this));
    }
}
