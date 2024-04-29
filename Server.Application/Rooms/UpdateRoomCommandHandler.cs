using Cord.Server.Domain;
using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.Rooms;

public class UpdateRoomCommandHandler : ICommandHandler<UpdateRoomCommand, IRoom> {
    readonly RoomProvider roomProvider;
    readonly ISanitizer sanitizer;
    readonly ResourceUploader resourceUploader;

    public UpdateRoomCommandHandler(
        RoomProvider roomProvider,
        ISanitizer sanitizer,
        ResourceUploader resourceUploader
    ) {
        this.roomProvider = roomProvider;
        this.sanitizer = sanitizer;
        this.resourceUploader = resourceUploader;
    }

    public async Task<IRoom> Handle(UpdateRoomCommand request, CancellationToken cancellationToken) {
        var model = request.UpdateRoom;
        var room = await roomProvider.GetRoom(request.RoomId);
        await room.EnsureHasPermissions(request.Sender, Permission.ManageServer);

        var update = new UpdateRoom(
            model.Name ?? room.Name,
            model.Description != null ? sanitizer.SanitizeHtml(model.Description) : room.Description,
            model.Categories ?? room.Categories.ToArray(),
            room.Icon,
            room.Banner,
            room.Wallpaper
        );

        if (model.Icon != null) {
            update = update with {
                Icon = model.Icon == "delete"
                    ? null
                    : await resourceUploader.UploadResource("icons", request.Sender, model.Icon)
            };
        }

        if (model.Banner != null) {
            update = update with {
                Banner = model.Banner == "delete"
                    ? null
                    : await resourceUploader.UploadResource("banners", request.Sender, model.Banner)
            };
        }

        if (model.Wallpaper != null) {
            update = update with {
                Wallpaper = model.Wallpaper == "delete"
                    ? null
                    : await resourceUploader.UploadResource("wallpapers", request.Sender, model.Wallpaper)
            };
        }

        await room.Update(update);
        return room;
    }
}
