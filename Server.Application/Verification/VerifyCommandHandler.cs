using Cord.Server.Application.Users;
using Cord.Server.Domain.Users;
using Cord.Server.Domain.Verification;
using MediatR;

namespace Cord.Server.Application.Verification;

public class VerifyCommandHandler : ICommandHandler<VerifyCommand> {
    readonly VerificationProvider verificationProvider;
    readonly UserProvider userProvider;

    public VerifyCommandHandler(VerificationProvider verificationProvider, UserProvider userProvider) {
        this.verificationProvider = verificationProvider;
        this.userProvider = userProvider;
    }

    public async Task<Unit> Handle(VerifyCommand request, CancellationToken cancellationToken) {
        var verification = await verificationProvider.PopByCode(request.Code, null);

        switch (verification.VerificationType) {
            case VerificationType.EmailVerification: {
                var user = await userProvider.GetUser(verification.UserId);
                await user.Verify(Verified.Email);

                return Unit.Value;
            }
            case VerificationType.SmsVerification: {
                var user = await userProvider.GetUser(verification.UserId);
                await user.Verify(Verified.SMS);

                return Unit.Value;
            }
            default:
                throw new BadRequestException();
        }
    }
}
