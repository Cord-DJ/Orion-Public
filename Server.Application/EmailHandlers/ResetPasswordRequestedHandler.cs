using Cord.Server.Domain;
using Cord.Server.Domain.Users;

namespace Cord.Server.Application.EmailHandlers;

public class ResetPasswordRequestedHandler : IHandler<ResetPasswordRequested> {
    readonly IEmailSender sender;

    public ResetPasswordRequestedHandler(IEmailSender sender) {
        this.sender = sender;
    }

    public async Task Handle(ResetPasswordRequested notification, CancellationToken cancellationToken) {
        if (notification.User is User user) {
            await sender.SendEmail(
                user.RecipientEmail,
                EmailType.PasswordRecovery,
                new { notification.User.Name, notification.Token }
            );
        }
    }
}
