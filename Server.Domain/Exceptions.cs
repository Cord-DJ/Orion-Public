namespace Cord.Server.Domain;

public sealed class PermissionsException : Exception {
    public Permission Permissions { get; }

    public PermissionsException(Permission permissions) {
        Permissions = permissions;
    }
}
