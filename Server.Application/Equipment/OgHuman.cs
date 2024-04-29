using Cord.Equipment;
using Cord.Server.Domain.Equipment;

namespace Cord.Server.Application.Equipment; 

public static class OgStuff {
    public static readonly Item OgHuman = new() {
        Id = ID.Parse("164410022376243300"),
        Name = "OG Human",
        Quality = ItemQuality.Event,
        Races = Array.Empty<ID>(),
        Modifications = HumanSkinColors.All.ToList(),
        Type = ItemType.Race,
        AssetName = "og_human",
        MinimumLevel = 0
    };

    public static readonly Item OgBasicTShirt = new() {
        Id = ID.Parse("164410022380437303"),
        Name = "Basic T-Shirt",
        Quality = ItemQuality.Poor,
        Races = new List<ID> { OgHuman.Id },
        Modifications = ItemColors.All.ToList(),
        Type = ItemType.Shirt,
        AssetName = "og_human_basic_tshirt",
        MinimumLevel = 0
    };

    public static readonly Item OgBasicPants = new() {
        Id = ID.Parse("164410022380437304"),
        Name = "Basic Pants",
        Quality = ItemQuality.Poor,
        Races = new List<ID> { OgHuman.Id },
        Modifications = ItemColors.All.ToList(),
        Type = ItemType.Legs,
        AssetName = "og_human_basic_pants",
        MinimumLevel = 0
    };

    public static readonly Item OgShortHair = new() {
        Id = ID.Parse("164410022380437305"),
        Name = "Short Hair",
        Quality = ItemQuality.Common,
        Races = new List<ID> { OgHuman.Id },
        Modifications = ItemColors.All.ToList(),
        Type = ItemType.Hair,
        AssetName = "og_human_short_hair",
        MinimumLevel = 0
    };
    
    public static readonly Item OgDick = new() {
        Id = ID.Parse("164410022380437306"),
        Name = "Dick",
        Quality = ItemQuality.Epic,
        Races = new List<ID> { OgHuman.Id },
        Modifications = Array.Empty<ItemModification>(),
        Type = ItemType.Waist,
        AssetName = "og_human_dick",
        MinimumLevel = 0
    };
    
    public static readonly Item OgGoldNecklace = new() {
        Id = ID.Parse("164410022380437307"),
        Name = "Gold Necklace",
        Quality = ItemQuality.Uncommon,
        Races = new List<ID> { OgHuman.Id },
        Modifications = Array.Empty<ItemModification>(),
        Type = ItemType.Neck,
        AssetName = "og_human_gold_necklace",
        MinimumLevel = 0
    };
    
    public static readonly Item OgSharingan = new() {
        Id = ID.Parse("164410022380437308"),
        Name = "Sharingan",
        Quality = ItemQuality.Epic,
        Races = new List<ID> { OgHuman.Id },
        Modifications = Array.Empty<ItemModification>(),
        Type = ItemType.Eyes,
        AssetName = "og_human_sharingan",
        MinimumLevel = 0
    };
}
