namespace Cord;

public interface IRole {
    ID Id { get; }
    int Position { get; }
    string Name { get; }
    string Color { get; }
    IRoleSettings Settings { get; }
    Permission Permissions { get; }

    public static bool operator >(IRole left, IRole right) => left.Position > right.Position;
    public static bool operator <(IRole left, IRole right) => left.Position < right.Position;
    public static bool operator >=(IRole left, IRole right) => left.Position >= right.Position;
    public static bool operator <=(IRole left, IRole right) => left.Position <= right.Position;
}
