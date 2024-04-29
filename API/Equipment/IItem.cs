namespace Cord.Equipment;

public interface IItem : ISnowflakeEntity {
    ItemType Type { get; }
    ItemQuality Quality { get; }
    string Name { get; }
    ICollection<ID> Races { get; }

    string AssetName { get; }
    IReadOnlyCollection<ItemModification> Modifications { get; }

    int MinimumLevel { get; }
    int? PriceLP { get; }
    int? priceCP { get; }
}
