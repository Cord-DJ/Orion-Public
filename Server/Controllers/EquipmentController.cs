using Cord.Equipment;
using Cord.Server.Application.Equipment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cord.Server.Controllers;

public partial class UsersController {
    [Authorize]
    [HttpGet("{id}/items")]
    public async Task<IEnumerable<IItemInstance>> GetItems(string id) {
        var sender = await ParseUserId(id, true);
        return await equipmentService.GetUserItemInstancesDto(sender).ToListAsync();
    }

    [Authorize]
    [HttpGet("{id}/presets")]
    public async Task<IEnumerable<IPreset>> GetPresets(string id) {
        var sender = await ParseUserId(id, true);
        return await equipmentService.GetUserPresetsDto(sender).ToListAsync();
    }

    [Authorize]
    [HttpPut("{userId}/presets/{id}")]
    public async Task<IPreset> UpdatePreset(string userId, int id, UpdatePresetModel model) {
        var sender = await ParseUserId(userId, true);
        return await mediator.Send(new UpdatePresetCommand(sender, id, model.Character));
    }

    [Authorize]
    [HttpDelete("{userId}/presets/{id}")]
    public async Task<IPreset> ResetPreset(string userId, int id) {
        var sender = await ParseUserId(userId, true);
        return await mediator.Send(new ResetPresetCommand(sender, id));
    }

    public record UpdatePresetModel(UpdateCharacter Character);
}
