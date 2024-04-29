namespace Cord;

public interface IBoost {
    DateTimeOffset FinishedTime { get; }
    int RemainingExp { get; }
}