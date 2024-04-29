namespace Cord;

public sealed record UpdateRoom(
    string? Name = null,
    string? Description = null,
    Category[]? Categories = null,
    string? Icon = null,
    string? Banner = null,
    string? Wallpaper = null
);
