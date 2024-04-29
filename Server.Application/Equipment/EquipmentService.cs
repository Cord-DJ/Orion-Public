using AutoMapper;
using Cord.Server.Domain.Equipment;
using Cord.Server.Domain.Users;

namespace Cord.Server.Application.Equipment;

public class EquipmentService {
    readonly IMapper mapper;
    readonly IItemInstanceRepository itemInstanceRepository;
    readonly IPresetRepository presetRepository;
    readonly IItemRepository itemRepository;

    public EquipmentService(
        IMapper mapper,
        IItemInstanceRepository itemInstanceRepository,
        IPresetRepository presetRepository,
        IItemRepository itemRepository
    ) {
        this.mapper = mapper;
        this.itemInstanceRepository = itemInstanceRepository;
        this.presetRepository = presetRepository;
        this.itemRepository = itemRepository;
    }

    public async IAsyncEnumerable<ItemInstanceDto> GetUserItemInstancesDto(User user) {
        await foreach (var x in itemInstanceRepository.GetForUser(user.Id)) {
            var dto = mapper.Map<ItemInstanceDto>(x);
            dto.Item = await GetItemDto(x.ItemId);

            yield return dto;
        }
    }

    public async IAsyncEnumerable<PresetDto> GetUserPresetsDto(User user) {
        await foreach (var x in presetRepository.GetForUser(user.Id)) {
            yield return await GetPresetDto(x);
        }
    }

    public async Task<PresetDto> GetPresetDto(Preset preset) {
        var dto = mapper.Map<PresetDto>(preset);
        dto.Character = await GetCharacterDto(preset.Character);

        return dto;
    }

    public async Task<ItemInstanceDto> GetItemInstance(ID id) {
        var instance = await itemInstanceRepository.Get(id);
        if (instance == null) {
            throw new NotFoundException(nameof(ItemInstance), id);
        }

        var dto = mapper.Map<ItemInstanceDto>(instance);
        dto.Item = await GetItemDto(instance.ItemId);

        return dto;
    }

    public async Task<ItemDto> GetItemDto(ID id) {
        var item = await itemRepository.Get(id);
        if (item == null) {
            throw new NotFoundException(nameof(Item), id);
        }

        return mapper.Map<ItemDto>(item);
    }

    public async Task<CharacterDto> GetCharacterDto(Character character) {
        var ret = new CharacterDto();
        foreach (var x in character) {
            ret[x.Key] = await GetItemInstance(x.Value);
        }

        return ret;
    }
}
