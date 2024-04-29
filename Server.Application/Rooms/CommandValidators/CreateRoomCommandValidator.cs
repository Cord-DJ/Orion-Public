using FluentValidation;

namespace Cord.Server.Application.Rooms.CommandValidators;

public sealed class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand> {
    public CreateRoomCommandValidator() {
        RuleFor(x => x.UpdateRoom.Name).NotEmpty().Length(3, 32);
        // TODO: category

        // 10 MB
        RuleFor(x => x.UpdateRoom.Icon).MaximumLength(10000000);
        RuleFor(x => x.UpdateRoom.Banner).MaximumLength(10000000);
    }
}
