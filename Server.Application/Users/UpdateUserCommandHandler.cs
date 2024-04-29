namespace Cord.Server.Application.Users;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, IUser> {
    readonly ResourceUploader resourceUploader;

    public UpdateUserCommandHandler(ResourceUploader resourceUploader) {
        this.resourceUploader = resourceUploader;
    }

    public async Task<IUser> Handle(UpdateUserCommand request, CancellationToken cancellationToken) {
        var model = request.UpdateUser;
        var user = request.User;

        var update = new UpdateUser(
            model.Name ?? user.Name,
            user.Discriminator,
            user.Avatar,
            user.Banner
        );

        if (model.Avatar != null) {
            update = update with {
                Avatar = model.Avatar == "delete"
                    ? null
                    : await resourceUploader.UploadResource("avatars", request.User, model.Avatar)
            };
        }

        if (model.Banner != null) {
            update = update with {
                Banner = model.Banner == "delete"
                    ? null
                    : await resourceUploader.UploadResource("banners", request.User, model.Banner)
            };
        }

        await user.Update(update);
        return user;
    }
}
