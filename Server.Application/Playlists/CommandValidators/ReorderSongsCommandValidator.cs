using FluentValidation;

namespace Cord.Server.Application.Playlists.CommandValidators;

public sealed class ReorderSongsCommandValidator : AbstractValidator<ReorderSongsCommand> {
    public ReorderSongsCommandValidator() {
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
