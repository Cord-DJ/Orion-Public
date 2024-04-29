using Cord.Equipment;

namespace Cord.Server.Application.Equipment;

public class PresetDto : IPreset {
    public ID Id { get; set; }
    public ICharacter Character { get; set; }
    public int Position { get; set; }

    public object[] PrimaryKeys { get; }
}
