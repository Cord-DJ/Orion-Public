namespace Cord.Equipment;

public interface IItemInstance : ISnowflakeEntity {
    IItem Item { get; }
    int? Count { get; }

    IReadOnlyCollection<ItemModification> AvailableModifications { get; set; }
    ItemModification? Modification { get; set; }
}
