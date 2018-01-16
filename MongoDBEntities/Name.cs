using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrwkEg.MongoDBEntities
{
    [BsonIgnoreExtraElements]
    public class Name
    {
        [BsonElement("first")]
        public string First { get; set; }
        [BsonElement("last")]
        public string Last { get; set; }
    }
}
