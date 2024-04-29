using FluentValidation;

namespace Cord.Server.Application.Users.CommandValidators;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand> {
    public ResetPasswordCommandValidator() {
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}
