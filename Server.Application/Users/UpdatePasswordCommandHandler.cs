using MediatR;

namespace Cord.Server.Application.Users;

public class UpdatePasswordCommandHandler : ICommandHandler<UpdatePasswordCommand> {
    public async Task<Unit> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken) {
        var user = request.User;

        if (!user.CheckPassword(request.CurrentPassword)) {
            throw new NotAllowedException();
        }

        await user.ChangePassword(request.NewPassword);
        return Unit.Value;
    }
}
