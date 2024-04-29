namespace Cord.Server.Domain.Users;

public sealed record Boost(
    DateTimeOffset FinishedTime,
    int RemainingExp
) : IBoost {
    public static readonly Boost InitialBoost = new(DateTimeOffset.UtcNow, 1000);
    public static readonly Boost Zero = new(DateTimeOffset.MaxValue, 0);
}