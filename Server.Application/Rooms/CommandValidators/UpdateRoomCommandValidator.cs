using FluentValidation;

namespace Cord.Server.Application.Rooms.CommandValidators;

public sealed class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand> {
    public UpdateRoomCommandValidator() {
        RuleFor(x => x.UpdateRoom.Name).NotEmpty().Length(3, 32);
        RuleFor(x => x.UpdateRoom.Description).MaximumLength(100000);
        // TODO: category

        // 10 MB
        RuleFor(x => x.UpdateRoom.Icon).MaximumLength(10000000);
        RuleFor(x => x.UpdateRoom.Banner).MaximumLength(10000000);
        RuleFor(x => x.UpdateRoom.Wallpaper).MaximumLength(10000000);
    }
}
