using Cord.Server.Domain.Users;

namespace Cord.Server.Domain.Rooms;

public sealed partial class Room {
    public bool IsMuted(ID userId) {
        model.Muted.RemoveAll(x => x.Expiration < DateTimeOffset.UtcNow);
        return model.Muted.Any(x => x.UserId == userId);
    }

    public bool IsBanned(ID userId) {
        model.Banned.RemoveAll(x => x.Expiration < DateTimeOffset.UtcNow);
        return model.Banned.Any(x => x.UserId == userId);
    }

    public async Task Ban(IUser user, TimeSpan? duration) {
        if (IsBanned(user.Id)) {
            return;
        }

        DateTimeOffset? expiration = duration != null ? DateTimeOffset.UtcNow.Add(duration.Value) : null;
        model.Banned.Add(new(user.Id, expiration));

        await Save();
        await Kick(user);
    }

    public Task Unban(IUser user) {
        model.Banned.RemoveAll(x => x.UserId == user.Id);

        return Save();
    }

    public Task Mute(IUser user, TimeSpan? duration) {
        if (IsMuted(user.Id)) {
            return Task.CompletedTask;
        }

        DateTimeOffset? expiration = duration != null ? DateTimeOffset.UtcNow.Add(duration.Value) : null;
        model.Muted.Add(new(user.Id, expiration));

        return Save();
    }

    public Task Unmute(IUser user) {
        model.Muted.RemoveAll(x => x.UserId == user.Id);
        return Save();
    }

    public async Task Kick(IUser user) {
        if (user is User u) {
            await RemoveOnlineUser(u);
            // await u.Buffer.Kick(Id, user);

            await SendInfoMessage($"{user.Name} has been kicked!");
        }
    }

    public async Task EnsureBannedUsersLoaded() {
        if (banned == null) {
            banned = new();

            foreach (var x in model.Banned) {
                var user = serviceProvider.GetRequiredService<User>();
                await user.Load(x.UserId);

                banned.Add(user.MinimalUser);
            }
        }
    }

    public async Task EnsureMutedUsersLoaded() {
        await EnsureMembersLoaded();

        if (muted == null) {
            muted = new();

            foreach (var x in model.Muted) {
                var local = members?.Find(u => u.User.Id == x.UserId);
                if (local != null) {
                    muted.Add(local.User);
                    continue;
                }

                var user = serviceProvider.GetRequiredService<User>();
                await user.Load(x.UserId);

                muted.Add(user.MinimalUser);
            }
        }
    }
}
