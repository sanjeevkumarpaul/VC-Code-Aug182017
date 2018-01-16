using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrwkEg.MongoDBEntities
{
    //Serializer to convert any Value to String. This goes as an BsonSerializer Attribute to any Collection Field. 
    public sealed class IDSerializer : SerializerBase<string>
    {
        public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.CurrentBsonType;
            switch (bsonType)
            {
                case BsonType.Null:
                    context.Reader.ReadNull();
                    return null;
                case BsonType.String:
                    return context.Reader.ReadString().ToString();
                case BsonType.Int32:
                    return context.Reader.ReadInt32().ToString();
                case BsonType.Double:
                    return context.Reader.ReadDouble().ToString();
                case BsonType.ObjectId:
                    return context.Reader.ReadObjectId().ToString();
                default:
                    var message = string.Format("Cannot deserialize BsonString or BsonInt32 from BsonType {0}.", bsonType);
                    throw new BsonSerializationException(message);
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
        {
            if (value != null)
            {
                context.Writer.WriteString(value.ToString());
            }
            else
            {
                context.Writer.WriteNull();
            }
        }
    }
}
