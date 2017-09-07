using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;


namespace MyEntityFramework
{
    public class EntityFrameworkCore : DbContext
    {
        private string _connectionStr;
        
        public EntityFrameworkCore(string connectionString) 
        {
            _connectionStr = connectionString;
        }

        public EntityFrameworkCore(DbContextOptions options, string connectionString) : base(options) 
        {
            _connectionStr = connectionString;            
        }

        public DbSet<MyData> Data {get;set;} //If fluent api does not privide the name for a table with reference to Enity under DbSet, this name has to match table name.
        public DbSet<MyKey> Keys {get;set;}

        #region ^events
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(this._connectionStr);
            Console.WriteLine($"Connected to : {this._connectionStr}" );
        }

        /// <summary>
        /// Adds flavor to Entity to Table name and many other with reference to Fluent API
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyKey>().ToTable("MyKeys");
            modelBuilder.Entity<MyData>().ToTable("MyData");
            
            //Establishing Relationships....
            modelBuilder.Entity<MyKey>()
                        .HasOne(k => k.MyData)
                        .WithMany(k => k.Keys )
                        .HasForeignKey(f => f.MyDataId );
        }

        #endregion

        public void WorkWithEntityFrameworkCore()
        {
             Console.WriteLine("Tryign to Learn Entity Framework Core.");
             Console.WriteLine("Is Sql Server :" + this.Database.IsSqlServer().ToString());
        }

    }

    /// <summary>
    /// This class will be mapped to table MyKeys.
    /// </summary>
    public class MyData 
    {
        public Int32 Id {get; set;}
        public string Name {get; set;}
        public DateTime Current {get; set;}
        public List<MyKey> Keys { get; set; } //Relationship
    }

    public class MyKey
    {
        public int Id {get; set;}
        public int MyDataId {get; set;}
        public string Department {get; set;}
        public DateTime Current {get; set;}
        public MyData MyData {get;set;} //For Relationship.

    }
}