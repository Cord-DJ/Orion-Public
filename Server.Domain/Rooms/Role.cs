namespace Cord.Server.Domain.Rooms;

public sealed record Role : IRole {
    public static readonly ID EveryoneId = new(420);

    public ID Id { get; }
    public int Position { get; set; }
    public string Name { get; init; }
    public string Color { get; init; }

    // display separately in users list
    // allow anyone to mention this role

    public IRoleSettings Settings { get; }
    public Permission Permissions { get; init; }

    public static Role Everyone =>
        new(
            EveryoneId,
            0,
            "everyone",
            "#99AABB",
            RoleSettings.Default,
            Permission.Everyone
        );

    public Role(ID id, int position, string name, string color, RoleSettings settings, Permission permissions) {
        Id = id;
        Position = position;
        Name = name;
        Color = color;
        Settings = settings;
        Permissions = permissions;
    }
}

public record RoleSettings(TimeSpan MaxSongLength) : IRoleSettings {
    public static RoleSettings Default => new(TimeSpan.FromMinutes(10));
}

// Audit log
// Server insights
