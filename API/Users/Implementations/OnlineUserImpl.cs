namespace Cord;

sealed class OnlineUserImpl : IOnlineUser, ISnowflakeEntity {
    public ID Id { get; }
    public IPosition Position { get; }

    // Users who not joined room
    public IUser? User { get; }

    public OnlineUserImpl(PositionImpl position, UserImpl? user) {
        Position = position;
        User = user;
    }

    public override bool Equals(object? obj) => obj is IOnlineUser entity && Id == entity.Id;
    public override int GetHashCode() => Id.GetHashCode();
}
