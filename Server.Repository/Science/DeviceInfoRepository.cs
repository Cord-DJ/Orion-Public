using Cord.Science;
using Cord.Server.Domain.Science;
using MongoDB.Bson.Serialization.Serializers;

namespace Cord.Server.Repository.Science;

public sealed class DeviceInfoRepository : Repository<DeviceInfo>, IDeviceInfoRepository {
    public DeviceInfoRepository(MongoContext mongoContext) : base(null, mongoContext) { }

    public static void CreateMapping() {
        BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<IDevice, Device>());

        BsonClassMap.RegisterClassMap<DeviceInfo>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.UserId).SetIsRequired(true);
                map.MapMember(x => x.DeviceAction).SetIsRequired(true);
                map.MapMember(x => x.Device);
                map.MapMember(x => x.ServerUserAgent);
                map.MapMember(x => x.IPAddress);
            }
        );
    }
}
