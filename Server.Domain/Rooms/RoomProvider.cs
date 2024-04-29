using Cord.Server.Domain.Users;

namespace Cord.Server.Domain.Rooms;

public sealed class RoomProvider {
    readonly IServiceProvider serviceProvider;
    readonly IRoomRepository roomRepository;
    readonly IOnlineRepository onlineRepository;
    readonly IQueueRepository queueRepository;
    readonly IMemberRepository memberRepository;

    public RoomProvider(
        IServiceProvider serviceProvider,
        IRoomRepository roomRepository,
        IOnlineRepository onlineRepository,
        IQueueRepository queueRepository,
        IMemberRepository memberRepository
    ) {
        this.serviceProvider = serviceProvider;
        this.roomRepository = roomRepository;
        this.onlineRepository = onlineRepository;
        this.queueRepository = queueRepository;
        this.memberRepository = memberRepository;
    }

    public async IAsyncEnumerable<Room> GetPopularRooms() {
        await foreach (var model in roomRepository.GetPopularRooms()) {
            var room = serviceProvider.GetRequiredService<Room>();
            room.Load(model);

            yield return room;
        }
    }

    public async IAsyncEnumerable<Room> GetRoomsWithExpiredSong() {
        await foreach (var model in roomRepository.GetRoomsWithExpiredSong()) {
            var room = serviceProvider.GetRequiredService<Room>();
            room.Load(model);

            yield return room;
        }
    }

    public async Task<Room> GetRoom(ID id) {
        var room = serviceProvider.GetRequiredService<Room>();
        await room.Load(id);

        return room;
    }

    public async Task<Room?> GetRoomByLink(string? link) {
        if (link == null) {
            return null;
        }

        try {
            var model = await roomRepository.GetRoomByLink(link);
            var room = serviceProvider.GetRequiredService<Room>();
            room.Load(model);

            return room;
        } catch (InvalidOperationException) {
            return null;
        }
    }

    // public async IAsyncEnumerable<Room> GetRoomsInfo(IEnumerable<ID> roomsId) {
    //     await foreach (var model in roomRepository.GetBasicRoomInfo(roomsId)) {
    //         var room = serviceProvider.GetRequiredService<Room>();
    //         room.Load(model);
    //
    //         yield return room;
    //     }
    // }

    public async Task<Room> CreateRoom(User owner, UpdateRoom inputModel) {
        var model = new RoomModel(
            ID.NewId(),
            owner.Id,
            inputModel.Name!,
            StringHelper.GenerateHash(11),
            null,
            new(),
            RoomFeature.None,
            inputModel.Icon,
            inputModel.Banner,
            null,
            null,
            new() { Role.Everyone },
            new(),
            new()
        );

        var room = serviceProvider.GetRequiredService<Room>();
        room.Load(model);

        await room.Save();
        await room.LoadAll();

        await room.AddMember(owner);
        await DomainEvents.Raise(new RoomCreated(room, owner));

        return room;
    }

    public async Task DeleteRoom(ID roomId) {
        var room = await GetRoom(roomId);

        await room.LoadAll();
        await DomainEvents.Raise(new RoomDeleted(room));

        await roomRepository.Remove(roomId);
        await roomRepository.RemoveByRoom(roomId);
        await onlineRepository.RemoveByRoom(roomId);
        await memberRepository.RemoveByRoom(roomId);
        await queueRepository.RemoveByRoom(roomId);
    }
}
