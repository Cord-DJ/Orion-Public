using Cord.Equipment;

namespace Cord.Server.Application.Equipment;

public class CharacterDto : Dictionary<SlotType, IItemInstance>, ICharacter { }
