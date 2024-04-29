using Cord.Equipment;

namespace Cord.Server.Domain.Equipment;

public interface IItemsManager {
    ICharacter CreateGuestCharacter(int discriminator);
    Task CreateUserDefaultInventory(ID userId);
    Task AddUserItem(ID userId, ID itemId);
}
