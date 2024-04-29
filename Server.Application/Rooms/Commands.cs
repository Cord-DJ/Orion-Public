using Cord.Server.Domain.Users;

namespace Cord.Server.Application.Rooms;

public record CreateRoomCommand(User Sender, UpdateRoom UpdateRoom) : ICommand<IRoom>;

public record UpdateRoomCommand(ID RoomId, User Sender, UpdateRoom UpdateRoom) : ICommand<IRoom>;

public record DeleteRoomCommand(ID RoomId, User Sender) : ICommand;

// Roles
public record CreateRoleCommand(ID RoomId, User Sender) : ICommand<IRole>;

public record DeleteRoleCommand(ID RoomId, User Sender, ID RoleId) : ICommand;

public record UpdateRoleCommand(ID RoomId, User Sender, ID RoleId, UpdateRole UpdateRole) : ICommand<IRole>;

// Admin
public record KickCommand(ID RoomId, User Sender, ID ReceiverId) : ICommand;

public record BanCommand(ID RoomId, User Sender, ID ReceiverId, int? Duration) : ICommand;

public record UnbanCommand(ID RoomId, User Sender, ID ReceiverId) : ICommand;

public record MuteCommand(ID RoomId, User Sender, ID ReceiverId, int? Duration) : ICommand;

public record UnmuteCommand(ID RoomId, User Sender, ID ReceiverId) : ICommand;

// Controls
public record ReorderQueueCommand(ID RoomId, User Sender, ID[] Order) : ICommand;

public record VoteCommand(ID RoomId, User Sender, string Value) : ICommand;

public record StealCommand(ID RoomId, User Sender, ID PlaylistId) : ICommand;

public record SkipCommand(ID RoomId, User Sender) : ICommand;

// Members
public record AddMemberCommand(ID RoomId, User User) : ICommand;

public record RemoveMemberCommand(ID RoomId, User User) : ICommand;

public record AddMemberRoleCommand(ID RoomId, User Sender, User User, ID RoleId) : ICommand;

public record RemoveMemberRoleCommand(ID RoomId, User Sender, User User, ID RoleId) : ICommand;

public record EnqueueCommand(ID RoomId, User Sender) : ICommand;

public record DequeueCommand(ID RoomId, User Sender, User User) : ICommand;
