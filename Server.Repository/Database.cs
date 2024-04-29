using Cord.Server.Domain.Equipment;
using Cord.Server.Domain.Messages;
using Cord.Server.Domain.Playlists;
using Cord.Server.Domain.Relationships;
using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Science;
using Cord.Server.Domain.Users;
using Cord.Server.Domain.Verification;
using Cord.Server.Repository.Equipment;
using Cord.Server.Repository.Messages;
using Cord.Server.Repository.Playlists;
using Cord.Server.Repository.Relationships;
using Cord.Server.Repository.Rooms;
using Cord.Server.Repository.Science;
using Cord.Server.Repository.Users;
using Cord.Server.Repository.Verification;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Conventions;

namespace Cord.Server.Repository;

public static class Database {
    public static void InitDatabase(this WebApplicationBuilder builder) {
        var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camelCase", conventionPack, _ => true);

        InitMappings();

        var options = builder.Configuration.GetSection(MongoDbOptions.Section).Get<MongoDbOptions>();
        var ctx = new MongoContext(
            options.Hostname,
            options.Collection
        );

        builder.Services.AddSingleton(ctx);

        builder.Services.Chain<IUserRepository>()
            // .Add<UserCacheRepository>()
            .Add<UserRepository>()
            .Configure();

        builder.Services.Chain<ISongRepository>()
            .Add<SongRepository>()
            .Configure();

        builder.Services.Chain<IRoomRepository>()
            .Add<RoomRepository>()
            .Configure();

        builder.Services.Chain<IPlaylistRepository>()
            .Add<PlaylistRepository>()
            .Configure();

        builder.Services.Chain<IMessageRepository>()
            .Add<MessageRepository>()
            .Configure();

        builder.Services.Chain<IOnlineRepository>()
            .Add<OnlineRepository>()
            .Configure();

        builder.Services.Chain<IQueueRepository>()
            .Add<QueueRepository>()
            .Configure();

        builder.Services.Chain<IGuestRepository>()
            .Add<GuestRepository>()
            .Configure();

        builder.Services.Chain<IMemberRepository>()
            // .Add<MemberCacheRepository>()
            .Add<MemberRepository>()
            .Configure();

        builder.Services.Chain<ISongsPlayedRepository>()
            .Add<SongsPlayedRepository>()
            .Configure();

        builder.Services.Chain<IVerificationRepository>()
            .Add<VerificationRepository>()
            .Configure();

        builder.Services.Chain<IDeviceInfoRepository>()
            .Add<DeviceInfoRepository>()
            .Configure();

        builder.Services.Chain<IItemRepository>()
            .Add<ItemCacheRepository>()
            .Add<ItemRepository>()
            .Configure();

        builder.Services.Chain<IPresetRepository>()
            .Add<PresetRepository>()
            .Configure();

        builder.Services.Chain<IItemInstanceRepository>()
            .Add<ItemInstanceRepository>()
            .Configure();

        builder.Services.Chain<IRelationshipRepository>()
            .Add<RelationshipRepository>()
            .Configure();
    }

    static void InitMappings() {
        UserRepository.CreateMapping();
        RoomRepository.CreateMapping();
        MessageRepository.CreateMapping();
        SongRepository.CreateMapping();
        PlaylistRepository.CreateMapping();
        OnlineRepository.CreateMapping();
        QueueRepository.CreateMapping();
        MemberRepository.CreateMapping();
        SongsPlayedRepository.CreateMapping();
        VerificationRepository.CreateMapping();
        DeviceInfoRepository.CreateMapping();
        ItemRepository.CreateMapping();
        PresetRepository.CreateMapping();
        ItemInstanceRepository.CreateMapping();
        RelationshipRepository.CreateMapping();
    }
}
