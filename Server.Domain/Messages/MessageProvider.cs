using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Users;

namespace Cord.Server.Domain.Messages;

public sealed class MessageProvider {
    readonly IServiceProvider serviceProvider;
    readonly IMessageRepository messageRepository;

    public MessageProvider(
        IServiceProvider serviceProvider,
        IMessageRepository messageRepository
    ) {
        this.serviceProvider = serviceProvider;
        this.messageRepository = messageRepository;
    }

    public async IAsyncEnumerable<IMessage> GetRoomMessages(Room room) {
        var models = await messageRepository.GetMessagesForRoom(room.Id);

        foreach (var model in models) {
            var msg = serviceProvider.GetRequiredService<Message>();
            await msg.Load(room, model);

            yield return msg;
        }
    }

    public async Task<IMessage> CreateMessage(Room room, IMember member, string text) {
        var model = new MessageModel(
            ID.NewId(),
            member.User.Id,
            room.Id,
            text,
            null,
            null,
            null
        );

        var msg = serviceProvider.GetRequiredService<Message>();

        await msg.Load(room, model);
        await messageRepository.Add(model);

        return msg;
    }

    public async Task<IMessage> CreateSystemMessage(Room room, string text) {
        var model = new MessageModel(
            ID.NewId(),
            MinimalUser.System.Id,
            room.Id,
            text,
            null,
            null,
            null
        );

        var msg = serviceProvider.GetRequiredService<Message>();
        await msg.Load(room, model);

        return msg;
    }
}
