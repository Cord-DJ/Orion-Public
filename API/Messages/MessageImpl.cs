using Newtonsoft.Json;

namespace Cord;

sealed class MessageImpl : IMessage, ISnowflakeEntity {
    public ID Id { get; init; }
    public IUser Author { get; init; }
    public IMember? Member { get; init; }
    public ID RoomId { get; init; }
    public string? Text { get; init; }
    public string? Sticker { get; init; }
    public IReadOnlyCollection<ID>? Mentions { get; init; }
    public IReadOnlyCollection<string>? MentionRoles { get; init; }

    [JsonConstructor]
    public MessageImpl(
        ID id,
        UserImpl? author,
        MemberImpl? member,
        ID roomId,
        string? text,
        string? sticker,
        List<ID>? mentions,
        List<string>? mentionRoles
    ) {
        Id = id;
        Author = author;
        Member = member; // TODO: member needs to be loaded
        RoomId = roomId;
        Text = text;
        Sticker = sticker;

        Mentions = mentions;
        MentionRoles = mentionRoles;
    }

    public async Task Delete() {
        var response = await CordClient.Http.DeleteAsync($"/api/rooms/{RoomId}/messages/{Id}");
        response.EnsureSuccessStatusCode();
    }
}
