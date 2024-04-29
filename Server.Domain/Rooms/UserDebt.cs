namespace Cord.Server.Domain.Rooms;

public sealed record UserDebt(ID UserId, DateTimeOffset? Expiration);
