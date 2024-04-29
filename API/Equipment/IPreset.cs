namespace Cord.Equipment;

public interface IPreset : ISnowflakeEntity {
    ICharacter Character { get; }

    int Position { get; }
}
