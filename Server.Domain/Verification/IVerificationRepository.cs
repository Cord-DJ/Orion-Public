namespace Cord.Server.Domain.Verification;

public interface IVerificationRepository : IRepository<Verification> {
    Task<Verification?> GetByCode(string code);
    IAsyncEnumerable<Verification> GetOutdated(TimeSpan timeSpan);
    Task RemoveBy(VerificationType type, ID userId);
}
