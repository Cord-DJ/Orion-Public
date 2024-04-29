namespace Cord.Server.Domain.Users;

public interface IUserRepository : IRepository<UserModel> {
    IAsyncEnumerable<UserModel> GetEveryone();
    Task<UserModel?> GetByEmail(string email);
    Task<UserModel?> GetByPassword(string password);
    Task<bool> Exists(string email);

    Task<long> GetActiveUsersCount(TimeSpan duration);
    Task<long> TotalUsers();
    Task<long> TotalVerifiedUsers();
}
