using Cord.Equipment;

namespace Cord.Server.Application.Equipment;

public class ItemInstanceDto : IItemInstance {
    public ID Id { get; set; }
    public IItem Item { get; set; }
    public int? Count { get; set; }

    public ItemModification? Modification { get; set; }
    public IReadOnlyCollection<ItemModification> AvailableModifications { get; set; }

    public object[] PrimaryKeys { get; }
}
