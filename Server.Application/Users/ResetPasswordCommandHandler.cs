using Cord.Server.Domain.Verification;
using MediatR;

namespace Cord.Server.Application.Users;

public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand> {
    readonly VerificationProvider verificationProvider;
    readonly UserProvider userProvider;

    public ResetPasswordCommandHandler(VerificationProvider verificationProvider, UserProvider userProvider) {
        this.verificationProvider = verificationProvider;
        this.userProvider = userProvider;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken) {
        var verification = await verificationProvider.PopByCode(request.Code, VerificationType.PasswordRecovery);
        var user = await userProvider.GetUser(verification.UserId);

        await user.ChangePassword(request.NewPassword);
        return Unit.Value;
    }
}
