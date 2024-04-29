namespace Cord.Server.Domain.Messages;

public sealed record MessageModel(
    ID Id,
    ID AuthorId,
    ID RoomId,
    string? Text,
    string? Sticker,
    List<ID>? Mentions,
    List<string>? MentionRoles
) : ISnowflakeEntity {
    public object[] PrimaryKeys => new object[] { Id };
}
