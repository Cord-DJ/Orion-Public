using Cord.Server.Domain;
using Cord.Server.Domain.Users;
using Cord.Server.Domain.Verification;

namespace Cord.Server.Application.EmailHandlers;

public sealed class UserCreatedHandler : IHandler<UserCreated> {
    readonly IEmailSender sender;
    readonly VerificationProvider verificationProvider;

    public UserCreatedHandler(IEmailSender sender, VerificationProvider verificationProvider) {
        this.sender = sender;
        this.verificationProvider = verificationProvider;
    }

    public async Task Handle(UserCreated notification, CancellationToken cancellationToken) {
        var token = await verificationProvider.CreateVerification(
            notification.User.Id,
            VerificationType.EmailVerification
        );

        if (notification.User is User user) {
            await sender.SendEmail(
                user.RecipientEmail,
                EmailType.EmailVerification,
                new { notification.User.Name, Token = token }
            );
        }
    }
}
