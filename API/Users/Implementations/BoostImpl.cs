namespace Cord;

sealed record BoostImpl : IBoost {
    public DateTimeOffset FinishedTime { get; init; }
    public int RemainingExp { get; init; }
}