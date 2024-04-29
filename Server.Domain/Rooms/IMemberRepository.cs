namespace Cord.Server.Domain.Rooms;

public interface IMemberRepository : IRepository<MemberModel> {
    IAsyncEnumerable<MemberModel> GetAllMembers(ID roomId);
    Task<long> GetMembersCount(ID roomId);
    IAsyncEnumerable<ID> GetJoinedRoomIds(ID userId);

    Task RemoveByRoom(ID roomId);
}
