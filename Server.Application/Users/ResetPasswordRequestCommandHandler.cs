using Cord.Server.Domain;
using Cord.Server.Domain.Verification;
using MediatR;

namespace Cord.Server.Application.Users;

public class ResetPasswordRequestCommandHandler : ICommandHandler<ResetPasswordRequestCommand> {
    readonly UserProvider userProvider;
    readonly VerificationProvider verificationProvider;

    public ResetPasswordRequestCommandHandler(UserProvider userProvider, VerificationProvider verificationProvider) {
        this.userProvider = userProvider;
        this.verificationProvider = verificationProvider;
    }

    public async Task<Unit> Handle(ResetPasswordRequestCommand request, CancellationToken cancellationToken) {
        var user = await userProvider.GetByEmail(request.Email);
        if (user == null) {
            throw new NotFoundException(nameof(IUser), null);
        }

        var code = await verificationProvider.CreateVerification(user.Id, VerificationType.PasswordRecovery);
        await DomainEvents.Raise(new ResetPasswordRequested(user, code));

        return Unit.Value;
    }
}
