using System;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Configuration;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LogConfigurations;

namespace MyMongoFramework
{
    public class MongoFrameworkCore 
    {
        private static readonly log4net.Core.ILogger log =  LoggingFactory.Logger(typeof(MongoFrameworkCore));
        
        //MongoClient _client;
        MongoServer _server;
        MongoDatabase _db;
 
        public MongoFrameworkCore( string server, int port, string database)
        {
            //var db1Credential = MongoCredential.CreateMongoCRCredential("db1", "uid", "pwd");
            //var db2Credential = MongoCredential.CreateMongoCRCredential("db2", "uid", "pwd");

            _server = new MongoServer( new MongoServerSettings()
            { 
                ConnectionMode = ConnectionMode.Automatic  ,
                Server = new MongoServerAddress(server, port),
                ClusterConfigurator = configure => 
                                      {
                                          configure.Subscribe<CommandStartedEvent>(e =>  ExecutionInterception(e) );
                                          configure.Subscribe<CommandSucceededEvent>(e => ExecuteSuccessInterception(e));
                                      }                                                
            });

            _db = _server.GetDatabase(database);                
        }

        public void ExecutionInterception( CommandStartedEvent e )
        {
                          
              log.Info($"Query Interpretation: {e.CommandName}");
              log.Info(e.Command.ToJson());              
        }
        public void ExecuteSuccessInterception(CommandSucceededEvent e)
        {
            //Console.WriteLine($"Query Interpretation: {e.CommandName}");
            //Console.WriteLine(e.ToJson() ); 
        }


        public IEnumerable<Zones> ReadAllDocuments()
        {
            if (_db != null)
            {
                return _db.GetCollection<Zones>("Zones").FindAll();
            }

            return null;
        }

        public IEnumerable<Zones> LimitedZones()
        {
            if (_db != null)
            {
                //either way
                //var query = Query.EQ("contribs","OOP" );
                //or
                var query = Query<Zones>.EQ(z =>  z.Contribs, "OOP" );
                
                
                return _db.GetCollection<Zones>("Zones").Find(query);
            }

            return null;
        }

        public IEnumerable<Zones> LimitedFields()
        {
            if (_db != null)
            {
                var query = Query<Zones>.EQ(z =>  z.name.First, "John" );
                
                //Excluding few fields.
                return _db.GetCollection<Zones>("Zones").Find(query).SetFields(Fields<Zones>.Exclude(f => f.name.First,
                                                                                                     f => f.Birth,
                                                                                                     f => f.Id )  );
            }

            return null;
        }

        public IEnumerable<Zones> MultipleConditions()
        {
            if (_db != null)
            {
                var queries = new QueryBuilder<Zones>();
                var query = queries.And( new List<IMongoQuery>
                                         { 
                                             Query<Zones>.EQ(z =>  z.name.First, "Kristen" ) ,
                                             Query<Zones>.EQ(z => z.Contribs, "OOP")
                                         } );
                query = queries.Or( query, Query<Zones>.ElemMatch(z => z.Awards, a => a.GT<double>( y => y.Year , 1971) ) ); //Element match requried for an Array.

                //Excluding few fields.
                return _db.GetCollection<Zones>("Zones").Find(query).SetFields(Fields<Zones>.Exclude(f => f.Birth,
                                                                                                     f => f.Id )  );
            }

            return null;
        }

        public IQueryable<Zones> FilterQuerable()
        {
            if (_db != null)
            {
                 var zones = _db.GetCollection<Zones>("Zones").AsQueryable();
                 
                 return zones.Where(z => z.name.First == "Kristen" );    //.Equals does not work instead use == always.

            }

            return null;
        }

        public IQueryable<Zones> FilterQuerableChaining()
        {
            if (_db != null)
            {
                 var zones = _db.GetCollection<Zones>("Zones").AsQueryable();
                 
                 return zones.Where(z => z.name.First != null).Skip(2).Take(4);    

            }

            return null;
        }
    }

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
                context.Writer.WriteString(value.ToString() );
            }
            else
            {
                context.Writer.WriteNull(); 
            }
       }
}


    //DOCUMENT TYPES (Nothing but Tables ...)
    //Remember that all Element names at MongoDB collection must match with collection class field defined below.
    //  Field name is matched via two ways . Either you specify BsonElement("<<exact case sensitive name>>") or name the field as Collection Item named at mongoDB just as name field below.
    
    [BsonIgnoreExtraElements]  //letting the deserialization know that the fields not in here are to be excluded. (eg. title field at mongo is excluded below)
    public class Zones
    {
        [BsonId] //Denotes auto generated NOSQL Id.
        [BsonSerializer(typeof( IDSerializer ))]   //Since _id in mongoDB contains various datatypes (nt just ObjectId, conversion is required while Serialization.)             
        public string Id { get; set; }
        
        //[BsonElement("name")]
        public Name name {get; set;}
        [BsonElement("birth")]
        public DateTime Birth {get; set;}
        [BsonElement("death")]
        public DateTime Death {get; set;}
        [BsonElement("contribs")]
        public string[] Contribs{get; set;}
        [BsonElement("awards")]
        public List<Awardee> Awards {get; set;}

    }

    [BsonIgnoreExtraElements]
    public class Name
    {
        [BsonElement("first")]
        public string First { get; set; }
        [BsonElement("last")]
        public string Last { get; set;}
    }

    [BsonIgnoreExtraElements]
    public class Awardee
    {
        [BsonElement("award")]
        public string Award {get; set;}
        [BsonElement("year")]
        public double Year {get; set;}
        [BsonElement("by")]
        public string By {get; set;}

    }
}