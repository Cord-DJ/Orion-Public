namespace Cord.Server.Domain.Equipment;

public class Preset : ISnowflakeEntity {
    int position;

    public object[] PrimaryKeys => new object[] { Id };

    public ID Id { get; } // TODO: we don't need this ID cuz we can use (UserId, Position) tuple
    public Character Character { get; private set; }
    public ID UserId { get; }

    public int Position {
        get => position;
        set {
            if (position is < 0 or > 3) {
                throw new ArgumentOutOfRangeException();
            }

            position = value;
        }
    }

    public Preset(ID id, ID userId, Character character) {
        Id = id;
        Character = character;
        UserId = userId;
    }

    public void StripOffAvatar() {
        Character = new();
    }
}
