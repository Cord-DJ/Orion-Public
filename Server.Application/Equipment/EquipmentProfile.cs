using AutoMapper;
using Cord.Equipment;
using Cord.Server.Domain.Equipment;

namespace Cord.Server.Application.Equipment;

public class EquipmentProfile : Profile {
    public EquipmentProfile() {
        CreateMap<Preset, PresetDto>()
            .ForMember(x => x.Character, opt => opt.Ignore());
        CreateMap<Item, ItemDto>();
        CreateMap<Item, IItem>().As<ItemDto>();
        CreateMap<ItemInstance, ItemInstanceDto>();
    }
}
