using Cord.Equipment;
using Cord.Server.Domain.Users;

namespace Cord.Server.Application.Equipment;

public record UpdatePresetCommand(User User, int PresetId, UpdateCharacter UpdateCharacter) : ICommand<IPreset>;

public record ResetPresetCommand(User User, int PresetId) : ICommand<IPreset>;
