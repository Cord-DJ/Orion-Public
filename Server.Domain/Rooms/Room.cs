using Cord.Server.Domain.Leveling;
using Cord.Server.Domain.Messages;
using Cord.Server.Domain.Playlists;
using Cord.Server.Domain.Users;

namespace Cord.Server.Domain.Rooms;

public sealed partial class Room : IRoom {
    readonly IRoomRepository roomRepository;
    readonly IUserProvider userProvider;
    readonly IMessageRepository messageRepository;
    readonly IOnlineRepository onlineRepository;
    readonly IQueueRepository queueRepository;
    readonly IServiceProvider serviceProvider;
    readonly IMemberRepository memberRepository;
    readonly MessageProvider messageProvider;
    readonly ISongsPlayedRepository songsPlayedRepository;
    readonly SongProvider songProvider;

    // Lazy loaded
    List<IOnlineUser>? onlineUsers;
    List<IMember>? members;
    List<IUser>? queue;
    List<IUser>? banned;
    List<IUser>? muted;
    List<IMessage>? messages;
    List<ISongPlayed>? songHistory;

    RoomModel model = default!;

    public ID Id => model.Id;
    public ID OwnerId => model.OwnerId;
    public string Name => model.Name;
    public string Link => model.Link;
    public string? Description => model.Description;
    public IReadOnlyCollection<Category> Categories => model.Categories ?? new();
    public RoomFeature Features => model.Features;
    public string? Icon => model.Icon;
    public string? Banner => model.Banner;
    public string? Wallpaper => model.Wallpaper;
    public ICurrentSong? CurrentSong { get; private set; }

    public IReadOnlyCollection<IRole> Roles => model.Roles.AsReadOnly();
    public IReadOnlyCollection<IOnlineUser>? OnlineUsers => onlineUsers?.AsReadOnly();
    public IReadOnlyCollection<IMember>? Members => members?.AsReadOnly();
    public IReadOnlyCollection<IUser>? Queue => queue?.AsReadOnly();

    public IReadOnlyCollection<IUser>? Banned => banned?.AsReadOnly();
    public IReadOnlyCollection<IUser>? Muted => muted?.AsReadOnly();
    public IReadOnlyCollection<IMessage>? Messages => messages?.AsReadOnly();

    public IReadOnlyCollection<ISongPlayed>? SongHistory => songHistory?.AsReadOnly();

    // These variables are set before reply based on specifi usecase
    public long? MemberCount { get; private set; }
    public long? OnlineCount { get; private set; }

    public Room(
        IRoomRepository roomRepository,
        IUserProvider userProvider,
        IMessageRepository messageRepository,
        IOnlineRepository onlineRepository,
        IQueueRepository queueRepository,
        IServiceProvider serviceProvider,
        IMemberRepository memberRepository,
        MessageProvider messageProvider,
        ISongsPlayedRepository songsPlayedRepository,
        SongProvider songProvider
    ) {
        this.roomRepository = roomRepository;
        this.userProvider = userProvider;
        this.messageRepository = messageRepository;
        this.onlineRepository = onlineRepository;
        this.queueRepository = queueRepository;
        this.serviceProvider = serviceProvider;
        this.memberRepository = memberRepository;
        this.messageProvider = messageProvider;
        this.songsPlayedRepository = songsPlayedRepository;
        this.songProvider = songProvider;
    }

    public bool IsDJ(IUser user) => CurrentSong?.UserId == user.Id;
    public long GetOnlineNonGuestUsersCount() => onlineRepository.GetOnlineNonGuestUsersCount(Id);

    public async Task<IMember> GetMember(ID userId) {
        await EnsureMembersLoaded();
        var member = members?.Find(x => x.User.Id == userId);

        if (member == null) {
            throw new NotFoundException(nameof(IMember), userId);
        }

        return member;
    }

    public async Task StopPlaying() {
        await LoadCurrentSongVotes();
        var oldSong = CurrentSong;

        model = model with { CurrentSong = null };
        CurrentSong = model.CurrentSong;

        await Save();
        await DomainEvents.Raise(new CurrentSongUpdated(this, oldSong, CurrentSong));
    }

    public Task RemoveFromQueue(IUser user) => RemoveFromQueue(user, false);

    public Task AddToQueue(IUser user) => AddToQueue((User)user, true);

    public async Task ReorderQueue(ID[] order) {
        var queue = await queueRepository.GetQueueForRoom(Id);
        if (order.Length != queue.Count) {
            throw new NotAllowedException();
        }

        if (order.Any(x => queue.All(y => y != x))) {
            throw new NotAllowedException();
        }

        await queueRepository.ReorderUsers(Id, order);
        await DomainEvents.Raise(new QueueUpdated(this));
    }

    public async Task EnsureHasOnlineUser(User user) {
        if (!await onlineRepository.HasOnlineUser(user.Id, Id)) {
            throw new("online user not found");
        }
    }

    public async Task Update(UpdateRoom update) {
        model = model with {
            Name = update.Name ?? model.Name,
            Description = update.Description,
            Categories = update.Categories?.ToList() ?? new(),
            Icon = update.Icon,
            Banner = update?.Banner,
            Wallpaper = update?.Wallpaper
        };

        await Save();
        await DomainEvents.Raise(new RoomUpdated(this));
    }

    public async Task LoadMembersCount() {
        OnlineCount = await onlineRepository.GetOnlineUsersCount(Id);
        MemberCount = await memberRepository.GetMembersCount(Id);
    }

    public async Task LoadCurrentSongVotes() {
        CurrentSong = await GetCurrentSongWithVotes();
    }

    public Task EnsureMembersLoaded() => members == null ? LoadMembers() : Task.CompletedTask;

    public async Task EnsureQueueLoaded(bool force = false) {
        await EnsureMembersLoaded();

        if (queue == null || force) {
            queue = (await queueRepository.GetQueueForRoom(Id))
                .Select(x => members!.Find(y => y.User.Id == x))
                .Where(x => x != null)
                .Select(x => (x!.User as User)!.MinimalUser)
                .ToList();
        }
    }

    public async Task EnsureMessagesLoaded() {
        await EnsureMembersLoaded();
        await LoadCurrentSongVotes();

        messages ??= await messageProvider.GetRoomMessages(this).ToListAsync();
    }

    public async Task EnsureOnlineUsersLoaded() {
        onlineUsers = new();
        await foreach (var (user, position) in GetOnlineUsers()) {
            onlineUsers.Add(new OnlineUser(user.Id, position, user is MinimalUser ? user : null));
        } 
    }

    public async Task LoadAll(User? forUser = null) {
        await EnsureQueueLoaded();
        await EnsureOnlineUsersLoaded();

        if (forUser != null && !IsBanned(forUser.Id) && AllowedAction(forUser, Permission.ReadMessageHistory)) {
            await EnsureMessagesLoaded();
        }

        await EnsureSongsHistoryLoaded();
    }

    public Task Save() => roomRepository.Update(model);

    public async Task RemoveOnlineUser(IUser user) {
        await RemoveFromQueue(user, true);
        await onlineRepository.RemoveActiveRoom(user.Id, Id);

        await DomainEvents.Raise(new OnlineUserRemoved(this, user.Id));
    }

    public async Task ForceNewSong() {
        var (dj, song) = await TakeNextSong();
        await PlaySong(dj, song);
    }

    public async Task<IMessage> SendMessage(IMember member, string message) {
        // var userRegex = new Regex(@"<@!\d{16}>*");
        // var matches = userRegex.Matches(message);
        // foreach (var m in matches) {
        //     Console.WriteLine("match " + m);
        // }

        // Log.Error("message user {@u}", member.User);
        var msg = await messageProvider.CreateMessage(this, member, message);
        await DomainEvents.Raise(new MessageCreated(this, msg));

        return msg;
    }

    public async Task SendInfoMessage(string message) {
        var msg = await messageProvider.CreateSystemMessage(this, message.Trim());
        await DomainEvents.Raise(new MessageCreated(this, msg));
    }

    public async Task DeleteMessage(IMessage message) {
        await messageRepository.Remove(message.Id);
        await DomainEvents.Raise(new MessageDeleted(this, message.Id));
    }

    public async Task AddMember(IUser user) {
        await EnsureMembersLoaded();
        if (members!.Any(x => x.User.Id == user.Id)) {
            throw new NotAllowedException();
        }

        var member = serviceProvider.GetRequiredService<Member>();
        var model = new MemberModel(
            ID.NewId(),
            Id,
            user.Id,
            null,
            null,
            new() { Role.EveryoneId },
            DateTimeOffset.UtcNow,
            null
        );

        member.Load(model, this, user);
        await memberRepository.Add(model);
        members!.Add(member);

        await DomainEvents.Raise(new MemberAdded(this, member));
    }

    public async Task RemoveMember(IUser user) {
        await EnsureMembersLoaded();
        if (members!.Find(x => x.User.Id == user.Id) is not Member member) {
            throw new NotFoundException(nameof(Member), user.Id);
        }

        await RemoveFromQueue(user, true);
        await memberRepository.Remove(member!.Id);
        members.Remove(member);

        await DomainEvents.Raise(new MemberRemoved(this, member));
    }

    public async Task Vote(IUser user, Vote vote) {
        await roomRepository.Vote(Id, user.Id, vote);
        await DomainEvents.Raise(new Voted(this, user, vote));
    }

    public Task<IDictionary<Vote, List<ID>>> GetVotes() => roomRepository.GetVotesForRoom(Id);

    public async Task DistributeExp(Func<ExpCalculator, User, ValueTask<int>> calculation) {
        await foreach (var (user, _) in GetOnlineUsers()) {
            if (user is User u) {
                if (u.IsGuest) {
                    continue;
                }

                var calculator = new ExpCalculator(u);
                await u.AddExp(await calculation(calculator, u));
            }
        }
    }

    public Task Delete() => throw new NotImplementedException();
    public Task DeleteRole(IRole role) => throw new NotImplementedException();
    public Task ReorderRoles(IRole[] roles) => throw new NotImplementedException();
    public Task Vote(Vote vote) => throw new NotImplementedException();
    public Task<IMessage> SendMessage(string message) => throw new NotImplementedException();

    public async Task Load(ID id) {
        var model = await roomRepository.Get(id);
        if (model == null) {
            throw new NotFoundException(nameof(Room), id);
        }

        Load(model);
    }

    public void Load(RoomModel model) {
        this.model = model;
        CurrentSong = model.CurrentSong;
    }

    async IAsyncEnumerable<(IUser, Position)> GetOnlineUsers() {
        await EnsureMembersLoaded();

        await foreach (var (id, position) in onlineRepository.GetOnlineUsers(Id)) {
            var local = members?.Find(m => m.User.Id == id);
            if (local != null) {
                yield return (local.User, position);
            } else {
                var user = serviceProvider.GetRequiredService<User>();
                await user.Load(id);
                await userProvider.LoadCharacter(user);

                yield return (user.MinimalUser, position); // TODO: is this ok?
            }
        }
    }

    async Task PlaySong(User dj, ISong song) {
        await LoadCurrentSongVotes();
        var oldSong = CurrentSong;

        model = model with {
            CurrentSong = new(
                (Song)song,
                dj.Id,
                DateTimeOffset.UtcNow.Add(song.Duration),
                new(),
                new(),
                new()
            )
        };
        CurrentSong = model.CurrentSong;

        Log.Information("playing next song {@Song}", CurrentSong);
        await roomRepository.ResetVotes(Id);
        await Save();

        await DomainEvents.Raise(new CurrentSongUpdated(this, oldSong, CurrentSong));
    }

    async Task<(User DJ, ISong Song)> TakeNextSong() {
        var newDJId = await queueRepository.PopNext(Id);
        // Log.Information("new song DJ {b}", newDJId);
        if (newDJId == null) {
            throw new QueueEmptyException();
        }

        var newDJ = await userProvider.GetUser(newDJId.Value);
        // Log.Information("new DJ {name} pl id {id}", newDJ.Name, newDJ.InternalActivePlaylistId);
        if (newDJ?.InternalActivePlaylistId == null) {
            return await TakeNextSong();
        }

        var playlist = await newDJ.GetActivePlaylist();
        // Log.Information("active Playlist {name}", playlist?.Name);
        if (playlist == null) {
            return await TakeNextSong();
        }

        var song = await playlist.PopNextSong();
        // Log.Information("next song {song}", song?.Name);
        if (song == null) {
            return await TakeNextSong();
        }

        await AddToQueue(newDJ, false);
        return (newDJ, song);
    }

    async Task RemoveFromQueue(IUser user, bool kickFromDJ) {
        await queueRepository.RemoveUser(Id, user.Id);

        if (kickFromDJ && IsDJ(user)) {
            try {
                await ForceNewSong();
            } catch (QueueEmptyException) {
                await StopPlaying();
            }
        }

        await DomainEvents.Raise(new QueueUpdated(this));
    }

    async Task AddToQueue(User user, bool force) {
        var isEmpty = await queueRepository.IsQueueEmpty(Id);
        var existing = await queueRepository.AddUser(Id, user.Id);

        // Send queue info to user who is in queue but not in room anymore
        if (existing != null) {
            var model = await roomRepository.Get(existing.Value);
            var room = serviceProvider.GetRequiredService<Room>();
            room.Load(model!);

            await DomainEvents.Raise(new QueueUpdated(room));
        }

        if (force && isEmpty && CurrentSong == null) {
            Log.Information($"queue is empty; {Name} starting DJ");
            await ForceNewSong();
        }

        await DomainEvents.Raise(new QueueUpdated(this));
    }

    async Task LoadMembers() {
        var models = await memberRepository.GetAllMembers(Id).ToListAsync();
        var users = await userProvider.GetMultiple(models.Select(x => x.UserId)).ToListAsync();

        members = new();
        foreach (var model in models) {
            var member = serviceProvider.GetRequiredService<Member>();
            var user = users.Find(x => x.Id == model.UserId);

            if (user == null) {
                Log.Warning(
                    "Loaded users {@Users} ids {@Ids}",
                    users.Select(x => x.Id.ToString()),
                    models.Select(x => x.UserId.ToString())
                );
                continue;
            }

            member.Load(model, this, user);
            members.Add(member);
        }
    }

    async Task EnsureSongsHistoryLoaded() {
        if (songHistory == null) {
            var songsPlayed = await songsPlayedRepository.GetForRoom(Id);
            var loaded = await songProvider.GetSongs(songsPlayed.Select(x => x.SongId)).ToListAsync();
            var users = await userProvider.GetUsers(songsPlayed.Select(x => x.UserId)).ToListAsync();

            songHistory = songsPlayed.Select(
                    x => new SongPlayed(
                        x.Id,
                        loaded.Find(y => y.Id == x.SongId)!,
                        users.Find(u => u.Id == x.UserId)!,
                        x.Upvotes,
                        x.Steals,
                        x.Downvotes
                    ) as ISongPlayed
                )
                .ToList();
        }
    }

    async Task<CurrentSong?> GetCurrentSongWithVotes() {
        var votes = await GetVotes();

        if (CurrentSong != null) {
            return new(
                (Song)CurrentSong.Song,
                CurrentSong.UserId,
                CurrentSong.EndTime,
                votes[Cord.Vote.Upvote],
                votes[Cord.Vote.Steal],
                votes[Cord.Vote.Downvote]
            );
        }

        return null;
    }
}

class QueueEmptyException : Exception { }
