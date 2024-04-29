using Cord.Science;

namespace Cord.Server.Domain.Science;

public enum DeviceAction {
    Registration,
    Login,
    Identify
}

public sealed record DeviceInfo(
    ID Id,
    ID UserId,
    DeviceAction DeviceAction,
    IDevice? Device,
    string? ServerUserAgent,
    string? IPAddress
) : ISnowflakeEntity {
    public object[] PrimaryKeys => new object[] { Id };
}
