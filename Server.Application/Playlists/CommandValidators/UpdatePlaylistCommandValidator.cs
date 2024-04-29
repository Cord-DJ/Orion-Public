using FluentValidation;

namespace Cord.Server.Application.Playlists.CommandValidators;

public sealed class UpdatePlaylistCommandValidator : AbstractValidator<UpdatePlaylistCommand> {
    public UpdatePlaylistCommandValidator() {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
