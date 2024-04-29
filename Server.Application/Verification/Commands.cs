using Cord.Server.Domain.Users;
using Cord.Server.Domain.Verification;

namespace Cord.Server.Application.Verification;

public record VerifyCommand(string Code) : ICommand;

public record CreateVerificationCommand(User Sender, VerificationType Type) : ICommand;
