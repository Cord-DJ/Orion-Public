using Cord.Server.Domain.Verification;
using MediatR;

namespace Cord.Server.Application.Users;

public class UpdateEmailCommandHandler : ICommandHandler<UpdateEmailCommand> {
    readonly VerificationProvider verificationProvider;
    readonly UserProvider userProvider;

    public UpdateEmailCommandHandler(VerificationProvider verificationProvider, UserProvider userProvider) {
        this.verificationProvider = verificationProvider;
        this.userProvider = userProvider;
    }

    public async Task<Unit> Handle(UpdateEmailCommand request, CancellationToken cancellationToken) {
        var verification = await verificationProvider.PopByCode(request.Code, VerificationType.EmailChange);
        var user = await userProvider.GetUser(verification.UserId);

        if (!user.CheckPassword(request.Password)) {
            throw new UnauthorizedException();
        }

        await user.ChangeEmail(request.NewEmail);
        return Unit.Value;
    }
}
