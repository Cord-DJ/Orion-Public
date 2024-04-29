namespace Cord.Server.Domain.Verification;

public sealed record Verification(
    ID Id,
    ID UserId,
    VerificationType VerificationType,
    string Code
) : ISnowflakeEntity {
    public object[] PrimaryKeys => new object[] { Id };
}
