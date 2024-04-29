using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Users;

namespace Cord.Server.Domain.Messages;

public sealed class Message : IMessage {
    readonly IUserProvider userProvider;
    MessageModel model = default!;

    public ID Id => model.Id;
    public IUser Author { get; private set; } = default!;
    public IMember? Member { get; private set; }
    public ID RoomId => model.RoomId;
    public string? Text => model.Text;
    public string? Sticker => model.Sticker;

    public IReadOnlyCollection<ID>? Mentions => model.Mentions?.AsReadOnly();
    public IReadOnlyCollection<string>? MentionRoles => model.MentionRoles?.AsReadOnly();

    public Message(IUserProvider userProvider) {
        this.userProvider = userProvider;
    }

    public Task Delete() => throw new NotImplementedException();

    internal async Task Load(Room room, MessageModel model) {
        await room.EnsureMembersLoaded();

        this.model = model;
        if (model.AuthorId == MinimalUser.System.Id) {
            Author = MinimalUser.System;
            return;
        }

        try {
            Member = await room.GetMember(model.AuthorId);
        } catch {
            Author = (await userProvider.GetUser(model.AuthorId)).MinimalUser;
        }
    }
}
