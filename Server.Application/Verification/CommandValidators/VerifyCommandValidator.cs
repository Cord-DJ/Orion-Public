using FluentValidation;

namespace Cord.Server.Application.Verification.CommandValidators;

public sealed class VerifyCommandValidator : AbstractValidator<VerifyCommand> {
    public VerifyCommandValidator() {
        RuleFor(x => x.Code).NotEmpty();
    }
}
