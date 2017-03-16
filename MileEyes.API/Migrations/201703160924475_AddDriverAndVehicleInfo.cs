namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDriverAndVehicleInfo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DriverInfoes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DriverId = c.Guid(nullable: false),
                        CompanyId = c.Guid(nullable: false),
                        CurrentMileage = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.VehicleInfoes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserId = c.Guid(nullable: false),
                        VehicleId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.VehicleInfoes");
            DropTable("dbo.DriverInfoes");
        }
    }
}
