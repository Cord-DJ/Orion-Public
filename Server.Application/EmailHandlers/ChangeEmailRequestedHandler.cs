using Cord.Server.Domain;
using Cord.Server.Domain.Users;

namespace Cord.Server.Application.EmailHandlers;

public class ChangeEmailRequestedHandler : IHandler<ChangeEmailRequested> {
    readonly IEmailSender sender;

    public ChangeEmailRequestedHandler(IEmailSender sender) {
        this.sender = sender;
    }

    public async Task Handle(ChangeEmailRequested notification, CancellationToken cancellationToken) {
        if (notification.User is User user && notification.Token != null) {
            await sender.SendEmail(
                user.RecipientEmail,
                EmailType.EmailChange,
                new { notification.User.Name, notification.Token }
            );
        }
    }
}
