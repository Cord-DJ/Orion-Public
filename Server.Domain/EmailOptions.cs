namespace Cord.Server.Domain;

public sealed class EmailOptions {
    public static readonly string Section = "email";

    public string Sender { get; init; } = default!;
    public string Name { get; init; } = default!;
}
