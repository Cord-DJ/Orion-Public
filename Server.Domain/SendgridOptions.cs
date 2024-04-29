namespace Cord.Server.Domain;

public sealed class SendgridOptions {
    public static readonly string Section = "sendgrid";

    public string ApiKey { get; init; } = default!;
}
