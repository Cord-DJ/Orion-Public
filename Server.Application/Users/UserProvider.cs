using Cord.Server.Application.Equipment;
using Cord.Server.Domain;
using Cord.Server.Domain.Equipment;
using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Users;

namespace Cord.Server.Application.Users;

public sealed class UserProvider : IUserProvider {
    readonly IServiceProvider serviceProvider;
    readonly IUserRepository userRepository;
    readonly IOnlineRepository onlineRepository;
    readonly IRoomRepository roomRepository;
    readonly IMemberRepository memberRepository;
    readonly EquipmentService equipmentService;
    readonly IItemsManager itemsManager;

    public UserProvider(
        IServiceProvider serviceProvider,
        IUserRepository userRepository,
        IOnlineRepository onlineRepository,
        IRoomRepository roomRepository,
        IMemberRepository memberRepository,
        EquipmentService equipmentService,
        IItemsManager itemsManager
    ) {
        this.serviceProvider = serviceProvider;
        this.userRepository = userRepository;
        this.onlineRepository = onlineRepository;
        this.roomRepository = roomRepository;
        this.memberRepository = memberRepository;
        this.equipmentService = equipmentService;
        this.itemsManager = itemsManager;
    }

    public Task<bool> ExistEmail(string email) => userRepository.Exists(email);

    public async IAsyncEnumerable<User> GetMultiple(IEnumerable<ID> ids) {
        await foreach (var x in userRepository.GetMultiple(ids)) {
            var user = serviceProvider.GetRequiredService<User>();
            user.Load(x);
            await LoadCharacter(user);

            yield return user;
        }
    }

    public async IAsyncEnumerable<Room> GetJoinedRooms(User user) {
        var ids = await memberRepository.GetJoinedRoomIds(user.Id).ToListAsync();

        await foreach (var x in roomRepository.GetMultiple(ids)) {
            var room = serviceProvider.GetRequiredService<Room>();
            room.Load(x);

            yield return room;
        }
    }

    public async IAsyncEnumerable<Room> GetActiveRooms(User user) {
        await foreach (var roomId in onlineRepository.GetByUser(user.Id)) {
            var room = serviceProvider.GetRequiredService<Room>();
            try {
                await room.Load(roomId);
            } catch {
                continue;
            }

            yield return room;
        }
    }

    public async IAsyncEnumerable<User> GetDisconnectingUsers(TimeSpan duration) {
        var online = await onlineRepository.GetDisconnectingUsers(duration).ToListAsync();

        await foreach (var x in GetMultiple(online)) {
            yield return x;
        }
    }

    public async Task<User?> GetByEmail(string email) {
        var model = await userRepository.GetByEmail(email.ToLower().Trim());
        if (model == null) {
            return null;
        }

        var user = serviceProvider.GetRequiredService<User>();
        user.Load(model);
        return user;
    }

    public async Task<User?> GetBotByToken(string token) {
        var model = await userRepository.GetByPassword(token);
        if (model == null) {
            return null;
        }

        var user = serviceProvider.GetRequiredService<User>();
        user.Load(model);
        return user.HasProperties(UserProperties.Bot) ? user : null;
    }

    public Task<long> GetActiveUsersCount(TimeSpan duration) => userRepository.GetActiveUsersCount(duration);

    public async Task<User> GetUser(ID id) {
        var user = serviceProvider.GetRequiredService<User>();
        await user.Load(id);

        return user;
    }

    public async IAsyncEnumerable<User> GetUsers(IEnumerable<ID> ids) {
        await foreach (var userModel in userRepository.GetMultiple(ids)) {
            var user = serviceProvider.GetRequiredService<User>();
            user.Load(userModel);

            yield return user;
        }
    }

    public async Task LoadCharacter(User user) {
        if (!user.IsGuest) {
            var presets = await equipmentService.GetUserPresetsDto(user).ToListAsync();
            user.Character = presets.First(x => x.Position == user.PresetPosition).Character;
        } else {
            user.Character = itemsManager.CreateGuestCharacter(user.Discriminator);
        }
    }

    public async Task<User> CreateUser(string email, string password, string name) {
        var model = new UserModel(
            ID.NewId(),
            email.ToLower().Trim(),
            StringHelper.HashPassword(password),
            UserProperties.None,
            name,
            User.NewDiscriminator(),
            null,
            null,
            null,
            0,
            0,
            1,
            0,
            0,
            Boost.InitialBoost,
            UserStats.Zero,
            Verified.None,
            DateTimeOffset.UtcNow
        );

        var user = serviceProvider.GetRequiredService<User>();
        user.Load(model);
        await user.Save();

        await itemsManager.CreateUserDefaultInventory(model.Id);
        await DomainEvents.Raise(new UserCreated(user));

        return user;
    }

    public async Task<User> CreateGuestUser() {
        var id = ID.NewId();

        var model = new UserModel(
            id,
            $"guest_{id}",
            "random_password_123",
            UserProperties.Guest,
            $"guest{id}",
            User.NewDiscriminator(),
            null,
            null,
            null,
            0,
            0,
            0,
            0,
            0,
            Boost.Zero,
            UserStats.Zero,
            Verified.None,
            DateTimeOffset.UtcNow
        );

        var user = serviceProvider.GetRequiredService<User>();
        user.Load(model);
        await user.Save();

        return user;
    }
}
