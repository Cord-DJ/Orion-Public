using Cord.Server.Domain;
using Cord.Server.Domain.Users;
using Cord.Server.Domain.Verification;

namespace Cord.Server.Application.Users;

public class
    UpdateEmailRequestCommandHandler : ICommandHandler<UpdateEmailRequestCommand, (string? Code, bool SendEmail)> {
    readonly VerificationProvider verificationProvider;

    public UpdateEmailRequestCommandHandler(VerificationProvider verificationProvider) {
        this.verificationProvider = verificationProvider;
    }

    public async Task<(string? Code, bool SendEmail)> Handle(
        UpdateEmailRequestCommand request,
        CancellationToken cancellationToken
    ) {
        var sendEmail = request.User.HasVerified(Verified.Email);
        var code = await verificationProvider.CreateVerification(request.User.Id, VerificationType.EmailChange);

        await DomainEvents.Raise(new ChangeEmailRequested(request.User, sendEmail ? code : null));
        return (code, sendEmail);
    }
}
