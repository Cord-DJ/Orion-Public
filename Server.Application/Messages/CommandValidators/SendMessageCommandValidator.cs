using FluentValidation;

namespace Cord.Server.Application.Messages.CommandValidators;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand> {
    public SendMessageCommandValidator() {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000000);
    }
}
