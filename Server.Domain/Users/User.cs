using Cord.Equipment;
using Cord.Server.Domain.Leveling;
using Cord.Server.Domain.Playlists;
using Cord.Server.Domain.Rooms;

namespace Cord.Server.Domain.Users;

public sealed class User : IUser {
    readonly PlaylistProvider playlistProvider;
    readonly IUserRepository userRepository;
    readonly IOnlineRepository onlineRepository;
    readonly IUserProvider userProvider;

    UserModel model = default!;
    List<IPlaylist>? playlists;

    public ID Id => model.Id;
    public UserProperties? Properties => model.Properties;
    public string Name => model.Name;
    public int Discriminator => model.Discriminator;
    public string? Avatar => model.Avatar;
    public string? Banner => model.Banner;
    public ICharacter? Character { get; set; }

    public int? Exp => !IsGuest ? model.Exp : null;
    public int? MaxExp => !IsGuest ? ExpCalculator.ExperienceForLevel(model.Level) : null;
    public int? Level => !IsGuest ? model.Level : null;
    public IBoost? Boost => !IsGuest ? model.Boost : null;
    public IUserStats? Stats => !IsGuest ? model.Stats : null;

    public IReadOnlyList<IPlaylist>? Playlists => playlists?.AsReadOnly();

    public string? Email { get; private set; }
    public ID? ActivePlaylistId { get; private set; } // Set this to the model only for @me
    public ID? InternalActivePlaylistId => model.ActivePlaylistId;

    // public bool HasBoost => Boost > 0;
    public string RecipientEmail => model.Email;
    public int PresetPosition => model.Preset;

    public bool IsGuest => HasProperties(UserProperties.Guest);

    // public TimeSpan OnlineTime =>TimeSpan.Zero; // TODO model.DisconnectTimer.Subtract(model.LastLoggedIn);
    public Verified Verified => model.Verified;

    public IUser MinimalUser => new MinimalUser(Id, Discriminator, Name) { Avatar = Avatar, Character = Character };

    public User(
        PlaylistProvider playlistProvider,
        IUserRepository userRepository,
        IOnlineRepository onlineRepository,
        IUserProvider userProvider
    ) {
        this.playlistProvider = playlistProvider;
        this.userRepository = userRepository;
        this.onlineRepository = onlineRepository;
        this.userProvider = userProvider;
    }

    public bool HasProperties(UserProperties properties) => (Properties & properties) == properties;

    public bool HasVerified(Verified verified) => (model.Verified & verified) == verified;

    public Task Verify(Verified verified) {
        model = model with { Verified = model.Verified | verified };
        return Save();
    }

    public bool CheckPassword(string password) => model.Password == StringHelper.HashPassword(password);

    public Task ChangePassword(string newPassword) {
        model = model with { Password = StringHelper.HashPassword(newPassword) };
        return Save();
    }

    public Task ChangeEmail(string newEmail) {
        model = model with { Email = newEmail, Verified = model.Verified & ~Verified.Email };

        return Save();
    }

    public async Task SetActiveRoom(Room room, bool isBot = false) {
        var positions = await onlineRepository.GetOnlineUsers(room.Id).ToListAsync();
        
        var exists = await onlineRepository.SetActiveRoom(Id, room.Id, Position.NewRandomPosition(), isBot);

        await room.LoadAll();
        await DomainEvents.Raise(new ActiveRoomSet(this, room, exists));
    }

    public async Task RemoveActiveRooms() {
        await foreach (var room in userProvider.GetActiveRooms(this)) {
            await room.RemoveOnlineUser(this);
        }
    }

    public Task<IPlaylist> CreatePlaylist(string name) => CreatePlaylist(name, false);

    public async Task<IPlaylist> CreatePlaylist(string name, bool isProcessing) {
        var playlist = await playlistProvider.CreatePlaylist(this, name, isProcessing);

        if (model.ActivePlaylistId == null) {
            model = model with { ActivePlaylistId = playlist.Id };
            await Save();
        }

        await DomainEvents.Raise(new PlaylistCreated(playlist));
        return playlist;
    }

    public Task<IPlaylist> ImportPlaylist(string id, ImportType type) =>
        throw
            // TODO: move code from controller here
            new NotImplementedException();

    public Task<Playlist> GetPlaylist(ID id) => playlistProvider.GetUserPlaylist(this, id);

    public async Task SetActivePlaylist(ID? id) {
        await EnsurePlaylistsLoaded();

        if (id == null) {
            model = model with { ActivePlaylistId = null };
        } else if (playlists!.Any(x => x.Id == id)) {
            model = model with { ActivePlaylistId = id };
        }

        SetPrivateFields();
        await Save();
        await DomainEvents.Raise(new ActivePlaylistSet(this));
    }

    public async Task DeletePlaylist(ID id) {
        await EnsurePlaylistsLoaded();

        var playlist = playlists!.Find(x => x.Id == id);
        if (playlist == null) {
            throw new NotFoundException(nameof(Playlist), id);
        }

        if (playlist.IsProcessing) {
            throw new NotAllowedException();
        }

        await playlist.Delete();

        if (model.ActivePlaylistId == id) {
            await SetActivePlaylist(playlists!.FirstOrDefault()?.Id);
        }
    }

    // This needs to be set before sending data to @me
    public void SetPrivateFields() {
        Email = model.Email;
        ActivePlaylistId = model.ActivePlaylistId;
    }

    public async Task EnsurePlaylistsLoaded() {
        if (playlists == null) {
            playlists = await playlistProvider.GetUserPlaylists(this).Select(x => x as IPlaylist).ToListAsync();
        }
    }

    public async Task Update(UpdateUser update) {
        // TODO: check if name + discriminator doesn't exists and if yes try the disc++ and iterate over
        model = model with {
            Name = update.Name ?? model.Name,
            Discriminator = update.Discriminator ?? model.Discriminator,
            Avatar = update.Avatar,
            Banner = update.Banner
        };

        await Save();
        await DomainEvents.Raise(new UserUpdated(this));
    }

    public Task Save() => userRepository.Update(model);

    public Task LogLoggedInTimer() {
        model = model with { LastLoggedIn = DateTimeOffset.UtcNow };
        return Save();
    }

    public Task<long> Ping() => onlineRepository.PingUser(Id, DateTimeOffset.UtcNow);

    public Task AddExp(int exp) {
        if (model.Boost.FinishedTime.AddDays(1) < DateTimeOffset.UtcNow) {
            model = model with {
                Boost = new Boost(
                    DateTimeOffset.UtcNow,
                    (int)Math.Round(ExpCalculator.ExperienceForLevel(model.Level) * 0.1)
                )
            };
        }

        if (model.Boost.RemainingExp == 0) {
            return Internal_AddExp(exp);
        }

        var ret = exp * 2;
        var remainingExp = model.Boost.RemainingExp - ret;
        if (remainingExp < 0) {
            ret += remainingExp / 2;
            remainingExp = 0;
        }

        model = model with { Boost = (Boost)model.Boost with { RemainingExp = remainingExp } };

        return Internal_AddExp(ret);
    }

    public Task UpdatePassword(string currentPassword, string newPassword) => throw new NotImplementedException();

    public static int NewDiscriminator() {
        var random = new Random();
        return random.Next(1, 9999);
    }

    public async Task Load(ID id) {
        var model = await userRepository.Get(id);
        if (model == null) {
            throw new NotFoundException(nameof(User), id);
        }

        Load(model);
    }

    public void Load(UserModel model) {
        this.model = model;
    }

    async Task Internal_AddExp(int exp) {
        if (model.Level >= ExpCalculator.MaxLevel) {
            return;
        }

        var newExp = model.Exp + exp;
        var newLevel = model.Level;

        if (newExp > ExpCalculator.ExperienceForLevel(model.Level)) {
            newExp %= ExpCalculator.ExperienceForLevel(model.Level);
            newLevel++;

            await DomainEvents.Raise(new LevelGained(this, newLevel));
        } else {
            if (exp > 0) {
                await DomainEvents.Raise(new ExperienceGained(this, exp));
            }
        }

        model = model with { Exp = newExp, Level = newLevel };

        await Save();
    }

    internal async Task<Playlist?> GetActivePlaylist() {
        if (model.ActivePlaylistId == null) {
            return null;
        }

        return await GetPlaylist(model.ActivePlaylistId.Value);
    }
}
