using FluentValidation;

namespace Cord.Server.Application.Playlists.CommandValidators;

public sealed class ImportPlaylistCommandValidator : AbstractValidator<ImportPlaylistCommand> {
    public ImportPlaylistCommandValidator() {
        RuleFor(x => x.PlaylistId).NotEmpty();
    }
}
