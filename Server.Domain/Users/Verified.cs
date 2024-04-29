namespace Cord.Server.Domain.Users;

[Flags]
public enum Verified {
    None = 0,
    Email = 1 << 0,
    SMS = 1 << 1
}
