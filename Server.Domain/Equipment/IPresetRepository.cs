namespace Cord.Server.Domain.Equipment;

public interface IPresetRepository : IRepository<Preset> {
    IAsyncEnumerable<Preset> GetForUser(ID userId);
    Task<Preset> GetPreset(ID userId, int position);
}
