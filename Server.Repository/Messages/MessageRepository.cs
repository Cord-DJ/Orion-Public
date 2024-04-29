using Cord.Server.Domain.Messages;

namespace Cord.Server.Repository.Messages;

public sealed class MessageRepository : Repository<MessageModel>, IMessageRepository {
    public MessageRepository(MongoContext context) : base(null, context) {
        var msgDef = Builders<MessageModel>.IndexKeys.Combine(
            Builders<MessageModel>.IndexKeys.Descending(x => x.RoomId)
        );

        Collection.Indexes.CreateOne(new CreateIndexModel<MessageModel>(msgDef));
    }

    public async Task<IReadOnlyCollection<MessageModel>> GetMessagesForRoom(ID roomId, int take = 50) {
        var messages = await Collection.Find(x => x.RoomId == roomId)
            .SortByDescending(x => x.Id)
            .Limit(take)
            .ToListAsync();
        // TODO: FIXME Use just Id instead of Id.Id but CompareTo seems ilegal
        return messages.OrderBy(x => x.Id.Id).ToList();
    }

    public Task<long> TotalMessages() {
        return Collection.CountDocumentsAsync(_ => true);
    }


    public static void CreateMapping() {
        BsonClassMap.RegisterClassMap<MessageModel>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.AuthorId).SetIsRequired(true);
                map.MapMember(x => x.RoomId).SetIsRequired(true);
                map.MapMember(x => x.Text);
                map.MapMember(x => x.Sticker);
                map.MapMember(x => x.Mentions);
                map.MapMember(x => x.MentionRoles);
            }
        );
    }
}
