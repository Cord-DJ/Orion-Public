using FluentValidation;

namespace Cord.Server.Application.Rooms.CommandValidators;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand> {
    public UpdateRoleCommandValidator() {
        RuleFor(x => x.UpdateRole.Name).NotEmpty().MaximumLength(32);
        RuleFor(x => x.UpdateRole.Color).NotEmpty();
    }
}
