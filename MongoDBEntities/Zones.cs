using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrwkEg.MongoDBEntities
{
   //DOCUMENT TYPES (Nothing but Tables ...)
    //Remember that all Element names at MongoDB collection must match with collection class field defined below.
    //  Field name is matched via two ways . Either you specify BsonElement("<<exact case sensitive name>>") or name the field as Collection Item named at mongoDB just as name field below.
    
    [BsonIgnoreExtraElements]  //letting the deserialization know that the fields not in here are to be excluded. (eg. title field at mongo is excluded below)
    public class Zones
    {
        [BsonId] //Denotes auto generated NOSQL Id.
        [BsonSerializer(typeof(IDSerializer))]   //Since _id in mongoDB contains various datatypes (nt just ObjectId, conversion is required while Serialization.)             
        public string Id { get; set; }

        //[BsonElement("name")]
        public Name name { get; set; }
        [BsonElement("birth")]
        public DateTime Birth { get; set; }
        [BsonElement("death")]
        public DateTime Death { get; set; }
        [BsonElement("contribs")]
        public string[] Contribs { get; set; }
        [BsonElement("awards")]
        public List<Awardee> Awards { get; set; }
    }

}
