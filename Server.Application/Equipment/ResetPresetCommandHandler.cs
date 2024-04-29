using Cord.Equipment;

namespace Cord.Server.Application.Equipment;

public class ResetPresetCommandHandler : ICommandHandler<ResetPresetCommand, IPreset> {
    public Task<IPreset> Handle(ResetPresetCommand request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
