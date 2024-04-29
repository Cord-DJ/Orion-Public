namespace Cord;

[Flags]
public enum RoomFeature {
    None = 0,
    Verified = 1 << 0,

    Partnered = 1 << 1
    // Discoverable = 1 << 1 // Can be discovered in explore
}
