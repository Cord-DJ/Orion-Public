using Cord.Server.Domain.Users;

namespace Cord.Server.Application.Users;

public record CreateUserCommand(string Email, string Password, string Name) : ICommand<IUser>;

public record UpdateUserCommand(User User, UpdateUser UpdateUser) : ICommand<IUser>;

public record UpdatePasswordCommand(User User, string CurrentPassword, string NewPassword) : ICommand;

public record UpdateEmailCommand(string Code, string Password, string NewEmail) : ICommand;

public record UpdateEmailRequestCommand(User User) : ICommand<(string? Code, bool SendEmail)>;

public record ResetPasswordRequestCommand(string Email) : ICommand;

public record ResetPasswordCommand(string Code, string NewPassword) : ICommand;
