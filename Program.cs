using System;
using System.Linq;
using System.IO;

using MyEntityFramework;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using MyMongoFramework;
using LogConfigurations;
using EntityFrwkEg.ConfigurationEntities;

namespace MyCore2
{
    class Program
    {
        static CustomConfigurations Config = null;
        private static readonly log4net.Core.ILogger log =  LoggingFactory.Logger(typeof(Program)); //Opened in VS2017
        static void Main(string[] args)
        {
            log.Info("Our Program Started...");
            
            InitializeCustomConfiguration();
            //TrySimpleQuery();
            //TryRelationshipQuery();
            TrySimpleMongoQuery();

            Console.ReadLine();
        }

        /// <summary>
        /// Initializing Configuration File with respect to customization of Application.
        /// </summary>
        private static void InitializeCustomConfiguration()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();  //Very important step.

            Config = new CustomConfigurations{ ConnectionStrings = new ConnectionStrings() , Smtp = new SmtpConfiguration() };
            configuration.Bind(Config);            
        }
        
        private static void TrySimpleMongoQuery()
        {
            var _config = Config.ConnectionStrings.MongoDb;

            //MongoFrameworkCore _context = new MongoFrameworkCore( _config.Server, 
            //                                                      _config.Port,
            //                                                      _config.Database ) ;

            var _context = new MongoFrameworkCore(_config);

            _context.SeedMongoData();


            //var zones = _context.ReadAllDocuments();
            //var zones = _context.LimitedZones();
            //var zones = _context.LimitedFields();
            //var zones = _context.MultipleConditions();
            var zones = _context.FilterQuerable();
            //var zones = _context.FilterQuerableChaining();
            
            foreach(var zone in zones)
            {
                Console.WriteLine($"Zone ID: {zone.Id} -> Name: {zone.name?.Last}, {zone.name?.First} -> Contribution Cnt: {zone.Contribs?.Count()}, Awards Cnt: {zone.Awards?.Count()}");
            }

            //_context.UpdateOneAwardee("Nidhi", "Technical Writer", "Group Instructor" );
            _context.UpdateAwardee("Nidhi", "Group Instructor", "Financial Author");
        }

        private static  void TrySimpleQuery()
        {
            using (var db = new  EntityFrameworkCore( Config.ConnectionStrings.SqlConnection ))
            {
                var _keys = from D in db.Data join 
                                 K in db.Keys on D.Id equals K.MyDataId
                                 where D.Id == 2
                                 select new
                                 { 
                                     D.Id,
                                     MyDataId = K.Id,
                                     K.Current,
                                     K.Department,
                                     D.Name
                                 }; 

                Console.WriteLine($"Keys Count = {_keys.Count()}");
                _keys.ForEachAsync( k => {
                    
                    Console.WriteLine($@"Data: 
                                         ID={k.Id}, 
                                         Key={k.MyDataId}, 
                                         Department={k.Department}, 
                                         Name={k.Name},
                                         Date={k.Current:MM-dd-yyyy}, 
                                         Time={k.Current:HH:mm:ss}");

                }).Wait();
            }
        }

        private static void TryRelationshipQuery()
        {
            using (var db = new  EntityFrameworkCore( Config.ConnectionStrings.SqlConnection ))
            {

                var _data = db.Data.Where(d => d.Id >= 3).Include("Keys");
                _data.ForEachAsync( d => {

                    Console.WriteLine("Count of Keys = " + d.Keys.Count);
                    Console.WriteLine($@"For - {d.Name}, Lets Put down Departments");

                    d.Keys.ForEach( k=> {  Console.WriteLine($@"Department: {k.Department} Created on {k.Current}");  });
                }).Wait();

            }
        }
    }

    
}
