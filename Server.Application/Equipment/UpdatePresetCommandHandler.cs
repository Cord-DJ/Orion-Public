using Cord.Equipment;
using Cord.Server.Domain;
using Cord.Server.Domain.Equipment;

namespace Cord.Server.Application.Equipment;

public class UpdatePresetCommandHandler : ICommandHandler<UpdatePresetCommand, IPreset> {
    readonly IPresetRepository presetRepository;
    readonly EquipmentService equipmentService;
    readonly IItemInstanceRepository itemInstanceRepository;

    public UpdatePresetCommandHandler(
        IPresetRepository presetRepository,
        EquipmentService equipmentService,
        IItemInstanceRepository itemInstanceRepository
    ) {
        this.presetRepository = presetRepository;
        this.equipmentService = equipmentService;
        this.itemInstanceRepository = itemInstanceRepository;
    }

    public async Task<IPreset> Handle(UpdatePresetCommand request, CancellationToken cancellationToken) {
        var preset = await presetRepository.GetPreset(request.User.Id, request.PresetId);
        var itemInstances = await itemInstanceRepository.GetForUser(request.User.Id).ToListAsync(cancellationToken);

        preset.StripOffAvatar();
        foreach (var slot in request.UpdateCharacter) {
            var instance = itemInstances.Single(x => x.Id == slot.Value.ItemInstanceId);
            if (slot.Value.Modification != null) {
                if (!instance.AvailableModifications.Contains(slot.Value.Modification)) {
                    throw new();
                }

                instance.Modification = slot.Value.Modification;
            }

            await itemInstanceRepository.Update(instance);
            preset.Character[slot.Key] = instance.Id;
        }

        // var race = itemInstances.Single(x => x.Id == preset.Character[SlotType.Race]);
        // foreach (var slot in Enum.GetValues<SlotType>()) {
        //     if (slot == SlotType.Race) {
        //         continue;
        //     }
        //     
        //     var item = itemInstances.Single(x => x.Id == preset.Character[slot]);
        //     // if (item.) check for races
        // }

        await presetRepository.Update(preset);
        await DomainEvents.Raise(
            new PresetChanged(request.User, preset, request.User.PresetPosition == preset.Position)
        );

        return await equipmentService.GetPresetDto(preset);
    }
}
