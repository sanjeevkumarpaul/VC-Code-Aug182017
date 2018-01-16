using System;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Configuration;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LogConfigurations;
using EntityFrwkEg.MongoDBEntities;

namespace MyMongoFramework
{
    public class MongoFrameworkCore 
    {
        private static readonly log4net.Core.ILogger log =  LoggingFactory.Logger(typeof(MongoFrameworkCore));
        private static readonly string _document = "Zones";
        
        //MongoClient _client;
        MongoServer _server;
        MongoDatabase _db;
 
        public MongoFrameworkCore( string server, int port, string database)
        {
            //var db1Credential = MongoCredential.CreateMongoCRCredential("db1", "uid", "pwd");
            //var db2Credential = MongoCredential.CreateMongoCRCredential("db2", "uid", "pwd");

            //Old way connectivity
            //====================
            //_server = new MongoServer( new MongoServerSettings()
            //{ 
            //    ConnectionMode = ConnectionMode.Automatic  ,
            //    Server = new MongoServerAddress(server, port),
            //    ClusterConfigurator = configure => 
            //                          {
            //                              configure.Subscribe<CommandStartedEvent>(e =>  ExecutionInterception(e) );
            //                              configure.Subscribe<CommandSucceededEvent>(e => ExecuteSuccessInterception(e));
            //                          }                                                
            //});

            //New way connectivity
            //====================
            var _set = MongoClientSettings.FromUrl(new MongoUrl($"mongodb://{server}:{port}"));
            _set.ConnectionMode = ConnectionMode.Automatic;
            _set.ClusterConfigurator = configure =>
                                      {
                                          configure.Subscribe<CommandStartedEvent>(e => ExecutionInterception(e));
                                          configure.Subscribe<CommandSucceededEvent>(e => ExecuteSuccessInterception(e));
                                      };
            _server = new MongoClient(_set).GetServer();

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
                return _db.GetCollection<Zones>(_document).FindAll();
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
                var query = Query<Zones>.EQ(z =>  z.Contribs, "Kanha Ka Bansuri");
                
                return _db.GetCollection<Zones>(_document).Find(query);
            }

            return null;
        }

        public IEnumerable<Zones> LimitedFields()
        {
            if (_db != null)
            {
                var query = Query<Zones>.EQ(z =>  z.name.First, "John" );
                
                //Excluding few fields.
                return _db.GetCollection<Zones>(_document).Find(query).SetFields(Fields<Zones>.Exclude(f => f.name.First,
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
                                             Query<Zones>.EQ(z => z.Contribs, "Rosemary Garden")
                                         } );
                query = queries.Or( query, Query<Zones>.ElemMatch(z => z.Awards, a => a.GT<double>( y => y.Year , 1971) ) ); //Element match requried for an Array.

                //Excluding few fields.
                return _db.GetCollection<Zones>(_document).Find(query).SetFields(Fields<Zones>.Exclude(f => f.Birth,
                                                                                                     f => f.Id )  ); //Sets the fields which you want to return
            }

            return null;
        }

        public IQueryable<Zones> FilterQuerable()
        {
            if (_db != null)
            {
                 var zones = _db.GetCollection<Zones>(_document).AsQueryable();
                 
                 return zones.Where(z => z.name.First == "Kristen" );    //.Equals does not work instead use == always.

            }

            return null;
        }

        public IQueryable<Zones> FilterQuerableChaining()
        {
            if (_db != null)
            {
                 var zones = _db.GetCollection<Zones>(_document).AsQueryable();
                 
                 return zones.Where(z => z.name.First != null).Skip(2).Take(4);    

            }

            return null;
        }

        public void UpdateOneAwardee(string firstName, string awardOldName, string awardNewName)
        {
            if (_db != null)
            {
                var _collection = _db.GetCollection<Zones>(_document);
                var _query = Query<Zones>.EQ(z => z.name.First, firstName);
                var zone =  _collection.FindOne(_query);

                if (zone != null)
                {
                    var award = zone.Awards.Find(a => a.Award.Equals(awardOldName));
                    award.Award = awardNewName;

                    _collection.Save(zone);
                }
            }
        }

        public void UpdateAwardee(string firstName, string awardOldName, string awardNewName)
        {
            if (_db != null)
            {
                var queries = new QueryBuilder<Zones>();
                var query = queries.And(new List<IMongoQuery>
                {
                     Query<Zones>.EQ(z =>  z.name.First, firstName ) ,
                     Query<Zones>.ElemMatch(z => z.Awards, a => a.EQ( y => y.Award,  awardOldName))
                });

                var _collection = _db.GetCollection<Zones>(_document);

                var zones = _collection.Find(query).ToList();

                if (zones.Count > 0)
                {
                    zones.ForEach(a => a.Awards.Where(x => x.Award.Equals(awardOldName)).ToList().ForEach(b => { b.Award = awardNewName; }));

                    zones.ForEach(z => { _collection.Save(z); });
                }
            }
        }

        public void SeedMongoData()
        {
            var options = CollectionOptions.SetCapped(false).SetMaxSize(5000).SetMaxDocuments(100);

            if (_db.CollectionExists(_document))
            {
                //_db.DropCollection(_document);

                try
                {
                    _db.CreateCollection(_document, options);

                    var _col = _db.GetCollection(_document);
                    _col.Insert<Zones>(new Zones
                    {
                        name = new Name { First = "John", Last = "Deen" },
                        Birth = new DateTime(1880, 1, 10),
                        Death = new DateTime(1956, 5, 17),
                        Contribs = new string[] { "Insight Out", "Rosemary Garden", "I Am Your Friend" },
                        Awards = new List<Awardee>
                    {
                        new Awardee { Award = "IFA", By="Doordarshan", Year = 1921 },
                        new Awardee { Award = "STAR CINI", By="Star", Year = 1923 },
                        new Awardee { Award = "ZEE PROVISONAL", By="Zee", Year = 1930 }
                    }
                    });

                    _col.Insert<Zones>(new Zones
                    {
                        Id = "Ind_1",
                        name = new Name { First = "Nidhi", Last = "Bansal" },
                        Birth = new DateTime(1820, 3, 11),
                        Death = new DateTime(1943, 2, 21),
                        Contribs = new string[] { "Pani Ki Pehali Boond", "Doopahar Ki Dhoop", "Kanha Ka Bansuri" },
                        Awards = new List<Awardee>
                    {
                        new Awardee { Award = "OSCAR", By="Hollywood", Year = 1922 },
                        new Awardee { Award = "TECHNICAL CARTOONIST", By="Star", Year = 1926 },
                        new Awardee { Award = "COREOGRAPHY", By="Zee", Year = 1942 }
                    }
                    });

                    _col.Insert<Zones>(new Zones
                    {
                        Id = "Hld_1",
                        name = new Name { First = "Kristen", Last = "Lopher" },
                        Birth = new DateTime(1870, 7, 9),
                        Death = new DateTime(1938, 7, 17),
                        Contribs = new string[] { "Rendeer In My Pond", "Rosemary Garden", "Whos is there?" },
                        Awards = new List<Awardee>
                    {
                        new Awardee { Award = "HOLLESTER", By="Hgtv", Year = 1918 },
                        new Awardee { Award = "STAR CINI", By="Star", Year = 1923 },
                        new Awardee { Award = "OSCAR", By="Hollywood", Year = 1936 }
                    }
                    });

                    _col.Insert<Zones>(new Zones
                    {
                        Id = "Ind_2",
                        name = new Name { First = "Rammohan", Last = "Basu" },
                        Birth = new DateTime(1923, 4, 30),
                        Death = new DateTime(1980, 3, 11),
                        Contribs = new string[] { "Pani Ki Pehali Boond", "My Lawn", "Please, Forget Me" },
                        Awards = new List<Awardee>
                    {
                        new Awardee { Award = "Fimfare", By="Bollywood", Year = 1952 },
                        new Awardee { Award = "Bharat Ratna", By="Govt Of India", Year = 1971 }
                    }
                    });
                }
                catch (MongoException e)
                {
                    log.Info("Collection already exists.");                    
                }
                catch(Exception e)
                {
                    log.Info("Error - ");
                    log.Error(e);
                }
                finally
                {
                    log.Info("Seeding to MongoDb is completed");
                }
            }
        }
    }

}