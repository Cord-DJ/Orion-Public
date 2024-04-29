namespace Cord.Server.Domain.Users;

public sealed record UserModel(
    ID Id,
    string Email,
    string Password,
    UserProperties Properties,
    string Name,
    int Discriminator,
    string? Avatar,
    string? Banner,
    ID? ActivePlaylistId,
    int Preset,
    int Exp,
    int Level,
    int LevelPoints,
    int CordPoints,
    IBoost Boost,
    IUserStats Stats,
    Verified Verified,
    DateTimeOffset LastLoggedIn
) : ISnowflakeEntity {
    public object[] PrimaryKeys => new object[] { Id };
}
