namespace Cord.Science;

public sealed record Device(
    string OperatingSystem,
    string Browser,
    string BrowserVersion,
    string UserAgent,
    int ScreenWidth,
    int ScreenHeight
) : IDevice;
