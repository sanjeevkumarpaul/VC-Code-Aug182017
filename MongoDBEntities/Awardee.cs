using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrwkEg.MongoDBEntities
{
    [BsonIgnoreExtraElements]
    public class Awardee
    {
        [BsonElement("award")]
        public string Award { get; set; }
        [BsonElement("year")]
        public double Year { get; set; }
        [BsonElement("by")]
        public string By { get; set; }
    }
}
