using Cord.Server.Domain.Users;
using Cord.Server.Domain.Verification;
using MediatR;

namespace Cord.Server.Application.Verification;

public class CreateVerificationCommandHandler : ICommandHandler<CreateVerificationCommand> {
    readonly IEmailSender emailSender;
    readonly VerificationProvider verificationProvider;

    public CreateVerificationCommandHandler(
        IEmailSender emailSender,
        VerificationProvider verificationProvider
    ) {
        this.emailSender = emailSender;
        this.verificationProvider = verificationProvider;
    }

    public async Task<Unit> Handle(CreateVerificationCommand request, CancellationToken cancellationToken) {
        var user = request.Sender;
        if (request.Type != VerificationType.EmailVerification) {
            throw new NotImplementedException();
        }

        if (user.HasVerified(Verified.Email)) {
            throw new NotAllowedException();
        }

        var token = await verificationProvider.CreateVerification(user.Id, request.Type);
        await emailSender.SendEmail(user.RecipientEmail, EmailType.EmailVerification, new { user.Name, Token = token });

        return Unit.Value;
    }
}
