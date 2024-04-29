using FluentValidation;

namespace Cord.Server.Application.Users.CommandValidators;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand> {
    public UpdateUserCommandValidator() {
        RuleFor(x => x.UpdateUser.Name).Length(3, 32);

        // 10 MB
        RuleFor(x => x.UpdateUser.Avatar).MaximumLength(10000000);
        RuleFor(x => x.UpdateUser.Banner).MaximumLength(10000000);
    }
}
