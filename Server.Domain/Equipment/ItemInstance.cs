using Cord.Equipment;

namespace Cord.Server.Domain.Equipment;

public class ItemInstance : ISnowflakeEntity {
    readonly List<ItemModification> availableModifications;

    public object[] PrimaryKeys => new object[] { Id };

    public ID Id { get; }
    public ID UserId { get; }
    public ID ItemId { get; }

    public int? Count { get; set; } // for consumables

    public ItemModification? Modification { get; set; } // TODO: only if it's in AvailableModification
    public IReadOnlyCollection<ItemModification> AvailableModifications => availableModifications.AsReadOnly();

    public ItemInstance(ID id, ID userId, ID itemId, IReadOnlyCollection<ItemModification> availableModifications) {
        Id = id;
        UserId = userId;
        ItemId = itemId;
        this.availableModifications = availableModifications.ToList();
    }

    public static ItemInstance CreateDefaultItemInstance(ID userId, Item item, bool copyModifications = false) {
        var instance = new ItemInstance(ID.NewId(), userId, item.Id, new List<ItemModification>());

        if ((item.Quality == ItemQuality.Poor || copyModifications) && item.Modifications.Count != 0) {
            // TODO: validate this
            instance.availableModifications.AddRange(item.Modifications);
            instance.Modification = item.Modifications.First();
        }

        return instance;
    }
}
