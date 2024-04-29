using Cord.Server.Domain;
using Cord.Server.Domain.Rooms;
using Rikarin.Core;

namespace Cord.Server.Repository.Rooms;

public class OnlineCacheRepository : IOnlineRepository {
    readonly List<OnlineModel> onlineModels = new();
    readonly ReaderWriterLockSlim lockSlim = new();
    public IRepository<OnlineModel, ID>? Next { get; }

    public Task<OnlineModel?> Get(ID id) {
        lockSlim.EnterReadLock();

        try {
            return Task.FromResult(onlineModels.FirstOrDefault(x => x.Id == id));
        } finally {
            lockSlim.ExitReadLock();
        }
    }

    public Task Add(OnlineModel value) {
        lockSlim.EnterWriteLock();

        try {
            if (
                !onlineModels.Any(x => x.Id == value.Id)
                && !onlineModels.Any(x => x.UserId == value.UserId && x.RoomId == value.RoomId)
            ) {
                onlineModels.Add(value);
            }
        } finally {
            lockSlim.ExitWriteLock();
        }
        
        return Task.CompletedTask;
    }

    public Task Update(OnlineModel value) {
        lockSlim.EnterWriteLock();

        try {
            var idx = onlineModels.FindIndex(x => x.Id == value.Id);
            if (idx != -1) {
                onlineModels[idx] = value;
            }
        } finally {
            lockSlim.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public Task Remove(OnlineModel value) => Remove(value.Id);

    public Task Remove(ID id) {
        lockSlim.EnterWriteLock();

        try {
            onlineModels.RemoveAll(x => x.Id == id);
        } finally {
            lockSlim.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public Task Remove(IEnumerable<ID> ids) {
        lockSlim.EnterWriteLock();

        try {
            onlineModels.RemoveAll(x => ids.Contains(x.Id));
        } finally {
            lockSlim.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<OnlineModel> GetMultiple(IEnumerable<ID> ids) {
        lockSlim.EnterReadLock();

        try {
            var entries = onlineModels.Where(x => ids.Contains(x.Id));
            // Not sure about deadlock
            foreach (var x in entries) {
                yield return x;
            }
        } finally {
            lockSlim.ExitReadLock();
        } 
    }

    public async IAsyncEnumerable<(ID, Position)> GetOnlineUsers(ID roomId) {
        lockSlim.EnterReadLock();

        try {
            var online = onlineModels.Where(x => x.RoomId == roomId);
            foreach (var x in online) {
                yield return (x.UserId, x.Position);
            }
        } finally {
            lockSlim.ExitReadLock();
        } 
    }

    public Task<long> GetOnlineUsersCount(ID roomId) {
        lockSlim.EnterReadLock();

        try {
            return Task.FromResult<long>(onlineModels.Count(x => x.RoomId == roomId));
        } finally {
            lockSlim.ExitReadLock();
        } 
    }

    public long GetOnlineNonGuestUsersCount(ID roomId) {
        lockSlim.EnterReadLock();

        try {
            // onlineModels.Count(x => x.RoomId == roomId && x.)
            return -1; // TODO
        } finally {
            lockSlim.ExitReadLock();
        }
    }

    public IAsyncEnumerable<ID> GetByUser(ID userId) => throw new NotImplementedException();

    public Task<bool> SetActiveRoom(ID userId, ID roomId, Position position, bool isBot) => throw new NotImplementedException();

    public Task<long> PingUser(ID userId, DateTimeOffset now) {
        // var online = Get(userId).Result;
        // if (online != null) {
        //     Update(online with { LastPing = now });
        // }
        throw new NotImplementedException();
    }

    public Task RemoveByRoom(ID roomId) {
        lockSlim.EnterWriteLock();

        try {
            onlineModels.RemoveAll(x => x.RoomId == roomId);
        } finally {
            lockSlim.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public Task RemoveActiveRoom(ID userId, ID roomId) {
        lockSlim.EnterWriteLock();

        try {
            onlineModels.RemoveAll(x => x.UserId == userId && x.RoomId == roomId);
        } finally {
            lockSlim.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<ID> GetDisconnectingUsers(TimeSpan duration) {
        lockSlim.EnterReadLock();

        try {
            var online = onlineModels.Where(x => x.LastPing < DateTimeOffset.UtcNow.Subtract(duration));
            foreach (var x in online) {
                yield return x.UserId;
            }
        } finally {
            lockSlim.ExitReadLock();
        }
    }

    public Task<bool> HasOnlineUser(ID userId, ID roomId) {
        lockSlim.EnterReadLock();

        try {
            return Task.FromResult(onlineModels.Any(x => x.UserId == userId && x.RoomId == roomId));
        } finally {
            lockSlim.ExitReadLock();
        }
    }

    public Task<long> TotalOnline() => Task.FromResult<long>(onlineModels.Count);
}
