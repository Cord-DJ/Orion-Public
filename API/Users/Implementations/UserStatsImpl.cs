namespace Cord;

sealed record UserStatsImpl : IUserStats {
    public int Woots { get; init; }
    public int DjPoints { get; init; }
}
