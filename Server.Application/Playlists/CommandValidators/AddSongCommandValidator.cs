using FluentValidation;

namespace Cord.Server.Application.Playlists.CommandValidators;

public sealed class AddSongCommandValidator : AbstractValidator<AddSongCommand> {
    public AddSongCommandValidator() {
        RuleFor(x => x.SongId).NotEmpty();
    }
}
