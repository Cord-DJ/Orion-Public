namespace Cord;

public interface IMessage {
    ID Id { get; }
    IUser Author { get; } // Minimal user
    IMember? Member { get; } // send as minimal member only if user is still member of the room
    ID RoomId { get; }
    string? Text { get; }
    string? Sticker { get; }

    IReadOnlyCollection<ID>? Mentions { get; }
    IReadOnlyCollection<string>? MentionRoles { get; }

    Task Delete();
}
