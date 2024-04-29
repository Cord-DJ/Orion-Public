using FluentValidation;

namespace Cord.Server.Application.Users.CommandValidators;

public sealed class UpdateEmailCommandValidator : AbstractValidator<UpdateEmailCommand> {
    public UpdateEmailCommandValidator() {
        RuleFor(x => x.NewEmail).NotEmpty().EmailAddress();
    }
}
