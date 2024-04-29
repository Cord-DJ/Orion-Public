using Cord.Equipment;

namespace Cord.Server.Application.Equipment;

public class ItemDto : IItem {
    public ID Id { get; set; }
    public ItemType Type { get; set; }
    public ItemQuality Quality { get; set; }
    public string Name { get; set; }
    public ICollection<ID> Races { get; set; }
    public string AssetName { get; set; }
    public IReadOnlyCollection<ItemModification> Modifications { get; set; }
    public int MinimumLevel { get; set; }
    public int? PriceLP { get; set; }
    public int? priceCP { get; set; }

    public object[] PrimaryKeys { get; }
}
