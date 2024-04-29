using Cord.Equipment;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Cord.Server.Repository.Equipment;

public class ItemModificationSerializer : SerializerBase<ItemModification> {
    public override ItemModification Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) {
        var reader = context.Reader;
        if (reader.GetCurrentBsonType() == BsonType.String) {
            return new(reader.ReadString());
        }

        if (reader.GetCurrentBsonType() == BsonType.Null) {
            reader.ReadNull();
            return null; // TODO: fix this
        }

        throw new InvalidCastException("wrong format");
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        ItemModification? value
    ) {
        if (value == null) {
            context.Writer.WriteNull();
        } else {
            context.Writer.WriteString(value.Value);
        }
    }
}
