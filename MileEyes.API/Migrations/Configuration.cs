using MileEyes.API.Models.DatabaseModels;

namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<MileEyes.API.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(MileEyes.API.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.FuelTypes.AddOrUpdate(ft => ft.Name,
                new FuelType() { Id = Guid.NewGuid(), Name = "PETROL" },
                new FuelType() { Id = Guid.NewGuid(), Name = "DIESEL" },
                new FuelType() { Id = Guid.NewGuid(), Name = "LPG" });

            context.SaveChanges();

            var fuelType = context.FuelTypes.FirstOrDefault(ft => ft.Name == "PETROL");

            context.EngineTypes.AddOrUpdate(et => et.Name,
                new EngineType()
                {
                    Id = Guid.NewGuid(),
                    FuelType = fuelType,
                    FuelRate = 0.07M,
                    Size = 1000,
                    Name = "Less than 1000c"
                });

            context.SaveChanges();
        }
    }
}
