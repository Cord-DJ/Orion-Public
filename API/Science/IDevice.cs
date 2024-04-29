namespace Cord.Science;

public interface IDevice {
    string OperatingSystem { get; }
    string Browser { get; }
    string BrowserVersion { get; }
    string UserAgent { get; }
    int ScreenWidth { get; }
    int ScreenHeight { get; }
}
