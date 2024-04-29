using Cord.Server.Domain.Rooms;

namespace Cord.Server.Application.Rooms;

public class CreateRoomCommandHandler : ICommandHandler<CreateRoomCommand, IRoom> {
    readonly ResourceUploader resourceUploader;
    readonly RoomProvider roomProvider;

    public CreateRoomCommandHandler(ResourceUploader resourceUploader, RoomProvider roomProvider) {
        this.resourceUploader = resourceUploader;
        this.roomProvider = roomProvider;
    }

    public async Task<IRoom> Handle(CreateRoomCommand request, CancellationToken cancellationToken) {
        var model = request.UpdateRoom;
        if (model.Icon != null) {
            model = model with {
                Icon = model.Icon == "delete"
                    ? null
                    : await resourceUploader.UploadResource("icons", request.Sender, model.Icon)
            };
        }

        if (model.Banner != null) {
            model = model with {
                Banner = model.Banner == "delete"
                    ? null
                    : await resourceUploader.UploadResource("banners", request.Sender, model.Banner)
            };
        }

        return await roomProvider.CreateRoom(request.Sender, model);
    }
}
