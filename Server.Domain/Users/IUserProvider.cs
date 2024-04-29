using Cord.Server.Domain.Rooms;

namespace Cord.Server.Domain.Users;

public interface IUserProvider {
    IAsyncEnumerable<User> GetMultiple(IEnumerable<ID> ids);
    Task<User> GetUser(ID id);
    IAsyncEnumerable<User> GetUsers(IEnumerable<ID> ids);
    IAsyncEnumerable<Room> GetActiveRooms(User user);
    Task LoadCharacter(User user);
}
