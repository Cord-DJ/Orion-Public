namespace Cord;

public sealed record UpdateRole(
    string Name,
    string Color,
    Permission Permissions
);
