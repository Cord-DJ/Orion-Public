using Cord.Equipment;
using Cord.Server.Domain.Equipment;
using Cord.Server.Domain.Users;

namespace Cord.Server.Application.Equipment;

public class ItemsManager : IItemsManager {
    readonly IItemRepository itemRepository;
    readonly IUserRepository userRepository;
    readonly IItemInstanceRepository itemInstanceRepository;
    readonly IPresetRepository presetRepository;

    static readonly Item Human = new() {
        Id = ID.Parse("164410022376243200"),
        Name = "Human",
        Quality = ItemQuality.Common,
        Races = Array.Empty<ID>(),
        Modifications = HumanSkinColors.All.ToList(),
        Type = ItemType.Race,
        AssetName = "human",
        MinimumLevel = 0
    };

    static readonly Item BasicTShirt = new() {
        Id = ID.Parse("164410022380437505"),
        Name = "Basic T-Shirt",
        Quality = ItemQuality.Poor,
        Races = new List<ID> { Human.Id },
        Modifications = ItemColors.All.ToList(),
        Type = ItemType.Shirt,
        AssetName = "human_basic_tshirt",
        MinimumLevel = 0
    };

    static readonly Item BasicPants = new() {
        Id = ID.Parse("164410022380437506"),
        Name = "Basic Pants",
        Quality = ItemQuality.Poor,
        Races = new List<ID> { Human.Id },
        Modifications = ItemColors.All.ToList(),
        Type = ItemType.Legs,
        AssetName = "human_basic_pants",
        MinimumLevel = 0
    };

    static readonly Item ShortHair = new() {
        Id = ID.Parse("164410022380437507"),
        Name = "Short Hair",
        Quality = ItemQuality.Common,
        Races = new List<ID> { Human.Id },
        Modifications = ItemColors.All.ToList(),
        Type = ItemType.Hair,
        AssetName = "human_short_hair",
        MinimumLevel = 0
    };

    public static readonly Item Raven = new() {
        Id = ID.Parse("164410022376243203"),
        Name = "Raven",
        Quality = ItemQuality.Legendary,
        Races = Array.Empty<ID>(),
        Modifications = Array.Empty<ItemModification>(),
        Type = ItemType.Race,
        AssetName = "raven",
        MinimumLevel = 0
    };

    public static readonly Item Batman = new() {
        Id = ID.Parse("164410022376243204"),
        Name = "Batman",
        Quality = ItemQuality.Legendary,
        Races = Array.Empty<ID>(),
        Modifications = Array.Empty<ItemModification>(),
        Type = ItemType.Race,
        AssetName = "batman",
        MinimumLevel = 0
    };

    static readonly Item Kid = new() {
        Id = ID.Parse("164410022376243205"),
        Name = "Kid",
        Quality = ItemQuality.Rare,
        Races = Array.Empty<ID>(),
        Modifications = Array.Empty<ItemModification>(),
        Type = ItemType.Race,
        AssetName = "kid",
        MinimumLevel = 0
    };


    static readonly List<Item> AllItems =
        new() {
            Human,
            BasicTShirt,
            BasicPants,
            ShortHair,

            // Rare
            Kid,

            // Legendary
            Raven,
            Batman,

            // Og Stuff
            OgStuff.OgHuman,
            OgStuff.OgBasicTShirt,
            OgStuff.OgBasicPants,
            OgStuff.OgShortHair,
            OgStuff.OgDick,
            OgStuff.OgSharingan,
            OgStuff.OgGoldNecklace
        };

    public ItemsManager(
        IItemRepository itemRepository,
        IUserRepository userRepository,
        IItemInstanceRepository itemInstanceRepository,
        IPresetRepository presetRepository
    ) {
        this.itemRepository = itemRepository;
        this.userRepository = userRepository;
        this.itemInstanceRepository = itemInstanceRepository;
        this.presetRepository = presetRepository;
    }

    public async Task Import() {
        foreach (var item in AllItems) {
            try {
                await itemRepository.Add(item);
            } catch (Exception e) {
                Log.Warning("Item {Name} already exists in DB. Skipping...", item.Name);
            }
        }

        await foreach (var user in userRepository.GetEveryone()) {
            if (user.Properties != UserProperties.Guest) {
                await CreateUserDefaultInventory(user.Id);
            }
        }
    }

    public async Task AddUserItem(ID userId, ID itemId) {
        var item = await itemRepository.Get(itemId);
        if (item == null) {
            throw new NotFoundException(nameof(Item), itemId);
        }
        
        await itemInstanceRepository.Add(ItemInstance.CreateDefaultItemInstance(userId, item, true));
    }

    public async Task CreateUserDefaultInventory(ID userId) {
        var human = ItemInstance.CreateDefaultItemInstance(userId, Human, true);
        var shortHair = ItemInstance.CreateDefaultItemInstance(userId, ShortHair, true);
        var basicTShirt = ItemInstance.CreateDefaultItemInstance(userId, BasicTShirt, true);
        var basicPants = ItemInstance.CreateDefaultItemInstance(userId, BasicPants, true);

        // await itemInstanceRepository.Add(ItemInstance.CreateDefaultItemInstance(userId, Raven));
        // await itemInstanceRepository.Add(ItemInstance.CreateDefaultItemInstance(userId, Batman));
        // await itemInstanceRepository.Add(ItemInstance.CreateDefaultItemInstance(userId, Kid));


        // Nobody else will gets the OG character after we transfer to new character system
        // OG Stuff
        // await itemInstanceRepository.Add(ItemInstance.CreateDefaultItemInstance(userId, OgStuff.OgHuman, true));
        // await itemInstanceRepository.Add(ItemInstance.CreateDefaultItemInstance(userId, OgStuff.OgBasicPants, true));
        // await itemInstanceRepository.Add(ItemInstance.CreateDefaultItemInstance(userId, OgStuff.OgBasicTShirt, true));
        // await itemInstanceRepository.Add(ItemInstance.CreateDefaultItemInstance(userId, OgStuff.OgShortHair, true));
        // await itemInstanceRepository.Add(ItemInstance.CreateDefaultItemInstance(userId, OgStuff.OgGoldNecklace));
        // // await itemInstanceRepository.Add(ItemInstance.CreateDefaultItemInstance(userId, OgStuff.OgDick));
        // await itemInstanceRepository.Add(ItemInstance.CreateDefaultItemInstance(userId, OgStuff.OgSharingan));

        await itemInstanceRepository.Add(human);
        await itemInstanceRepository.Add(shortHair);
        await itemInstanceRepository.Add(basicTShirt);
        await itemInstanceRepository.Add(basicPants);

        var character = new Character {
            { SlotType.Race, human.Id },
            { SlotType.Hair, shortHair.Id },
            { SlotType.Shirt, basicTShirt.Id },
            { SlotType.Legs, basicPants.Id }
        };

        var preset1 = new Preset(ID.NewId(), userId, character) { Position = 0 };
        await presetRepository.Add(preset1);
    }

    public ICharacter CreateGuestCharacter(int discriminator) {
        var random = new Random(discriminator);

        var race = new ItemInstanceDto { Id = ID.NewId(), Item = Human, Modification = HumanSkinColors.White };
        var shortHair = new ItemInstanceDto { Id = ID.NewId(), Item = ShortHair, Modification = ItemColors.Orange };

        var basicTShirt = new ItemInstanceDto {
            Id = ID.NewId(),
            Item = BasicTShirt,
            Modification = ItemColors.All.ToArray()[random.Next(ItemColors.All.Count())]
        };

        var basicPants = new ItemInstanceDto {
            Id = ID.NewId(),
            Item = BasicPants,
            Modification = ItemColors.All.ToArray()[random.Next(ItemColors.All.Count())]
        };

        var character = new CharacterDto {
            { SlotType.Race, race },
            { SlotType.Hair, shortHair },
            { SlotType.Shirt, basicTShirt },
            { SlotType.Legs, basicPants }
        };

        return character;
    }
}
