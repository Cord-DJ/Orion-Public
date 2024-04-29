namespace Cord.Server.Domain.Rooms;

public sealed record MemberModel(
    ID Id,
    ID RoomId,
    ID UserId,
    string? Nick,
    string? Avatar,
    List<ID> Roles,
    DateTimeOffset JoinedAt,
    DateTimeOffset? BoostingSince
) : ISnowflakeEntity {
    public object[] PrimaryKeys => new object[] { Id };
}
