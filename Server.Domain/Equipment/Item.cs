using Cord.Equipment;

namespace Cord.Server.Domain.Equipment;

// TODO: this can be record
public sealed class Item : IItem {
    public object[] PrimaryKeys => new object[] { Id };

    public ID Id { get; init; }
    public ItemType Type { get; init; }
    public ItemQuality Quality { get; init; }
    public string Name { get; init; }
    public ICollection<ID> Races { get; init; }

    public string AssetName { get; init; }

    public int MinimumLevel { get; init; }
    public int? PriceLP { get; init; }
    public int? priceCP { get; init; }

    public IReadOnlyCollection<ItemModification> Modifications { get; init; }
}
