namespace Cord.Server.Domain.Users;

public sealed record UserStats(
    int Woots,
    int DjPoints
) : IUserStats {
    public static readonly UserStats Zero = new(0, 0);
}
