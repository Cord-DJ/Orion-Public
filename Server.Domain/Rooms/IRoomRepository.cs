namespace Cord.Server.Domain.Rooms;

public interface IRoomRepository : IRepository<RoomModel> {
    IAsyncEnumerable<RoomModel> GetAllRooms();
    IAsyncEnumerable<RoomModel> GetPopularRooms();
    IAsyncEnumerable<RoomModel> GetRoomsWithExpiredSong();
    Task<long> PopularRoomsCount();
    Task<RoomModel> GetRoomByLink(string? link);
    Task<bool> ExistsLink(string? link);
    IAsyncEnumerable<RoomModel> GetBasicRoomInfo(IEnumerable<ID> roomsId);

    // Votes
    Task Vote(ID roomId, ID userId, Vote vote);
    Task ResetVotes(ID roomId);
    Task<IDictionary<Vote, List<ID>>> GetVotesForRoom(ID roomId);

    Task RemoveByRoom(ID roomId);
}
