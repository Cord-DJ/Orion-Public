namespace Cord;

sealed class RoleImpl : IRole, ISnowflakeEntity {
    public ID Id { get; }
    public int Position { get; }
    public string Name { get; }
    public string Color { get; }

    public IRoleSettings Settings { get; }
    public Permission Permissions { get; }

    public RoleImpl(
        ID id,
        int position,
        string name,
        string color,
        RoleSettingsImpl settings,
        Permission permissions
    ) {
        Id = id;
        Position = position;
        Name = name;
        Color = color;
        Settings = settings;
        Permissions = permissions;
    }

    public override string ToString() => $"<@&{Id}>";
    public override bool Equals(object? obj) => obj is IRole entity && Id == entity.Id;
    public override int GetHashCode() => Id.GetHashCode();
}
