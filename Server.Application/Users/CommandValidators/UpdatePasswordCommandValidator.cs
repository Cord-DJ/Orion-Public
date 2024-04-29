using FluentValidation;

namespace Cord.Server.Application.Users.CommandValidators;

public sealed class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand> {
    public UpdatePasswordCommandValidator() {
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}
