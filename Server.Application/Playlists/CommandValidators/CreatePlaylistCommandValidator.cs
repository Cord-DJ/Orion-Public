using FluentValidation;

namespace Cord.Server.Application.Playlists.CommandValidators;

public sealed class CreatePlaylistCommandValidator : AbstractValidator<CreatePlaylistCommand> {
    public CreatePlaylistCommandValidator() {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
