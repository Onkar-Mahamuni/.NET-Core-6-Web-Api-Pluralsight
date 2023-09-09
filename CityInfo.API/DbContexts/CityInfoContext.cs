using CityInfo.API.Entities;
using CityInfo.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.DbContexts
{
    public class CityInfoContext : DbContext
    {
        public DbSet<City> Cities { get; set; } = null!; // Null forgiving operator

        public DbSet<PointOfInterest> PointOfInterests { get; set; } = null!;

        // Way 1. To initialize the database- To let it know where to find this database
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlite("connectionString");
        //    base.OnConfiguring(optionsBuilder);
        //}

        //Way 2. To initialize the database- To let it know where to find this database
        public CityInfoContext(DbContextOptions<CityInfoContext> options) : base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<City>().HasData(
                new City("New York City")
                {
                    Id = 1,
                    Description = "The one with that big park."
                },
                new City("Antwrep")
                {
                    Id = 2,
                    Description = "The one with cathedral that was never really finished."
                },
                new City("Paris")
                {
                    Id = 3,
                    Description = "The one with that big tower."
                });

            modelBuilder.Entity<PointOfInterest>()
                .HasData(
                    new PointOfInterest("Central Park")
                    {
                        Id = 1,
                        CityId = 1,
                        Description = "Most visited urban park in US"
                    },
                    new PointOfInterest("Empire State Building")
                    {
                        Id = 2,
                        CityId = 1,
                        Description = "102 Story"
                    },
                    new PointOfInterest("Antwerp Park")
                    {
                        Id = 3,
                        CityId = 2,
                        Description = "Most visited urban park in Antwerp"
                    },
                    new PointOfInterest("Antwerp Building")
                    {
                        Id = 4,
                        CityId = 2,
                        Description = "102 Story"
                    },
                    new PointOfInterest("Eiffel Park")
                    {
                        Id = 5,
                        CityId = 3,
                        Description = "Most visited urban park in France"
                    },
                    new PointOfInterest("Eiffel Tower")
                    {
                        Id = 6,
                        CityId = 3,
                        Description = "50 Story"
                    }
                );


            base.OnModelCreating(modelBuilder);
        }
    }
}
