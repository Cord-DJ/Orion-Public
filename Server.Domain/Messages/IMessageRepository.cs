namespace Cord.Server.Domain.Messages;

public interface IMessageRepository : IRepository<MessageModel> {
    Task<IReadOnlyCollection<MessageModel>> GetMessagesForRoom(ID roomId, int take = 50);
    Task<long> TotalMessages();
}
