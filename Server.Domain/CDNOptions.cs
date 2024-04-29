namespace Cord.Server.Domain;

public sealed class CDNOptions {
    public static readonly string Section = "cdn";

    public string Endpoint { get; init; } = default!;
    public string AuthToken { get; init; } = default!;
}
