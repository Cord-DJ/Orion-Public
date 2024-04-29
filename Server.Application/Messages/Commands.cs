using Cord.Server.Domain.Users;

namespace Cord.Server.Application.Messages;

public record SendMessageCommand(ID RoomId, User Sender, string Message) : ICommand<IMessage>;

public record DeleteMessageCommand(ID RoomId, User Sender, ID MessageId) : ICommand;
