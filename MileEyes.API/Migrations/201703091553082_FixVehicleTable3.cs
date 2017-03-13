namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixVehicleTable3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VehicleTypes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        RegDate = c.DateTimeOffset(nullable: false, precision: 7),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Vehicles", "VehicleType_Id", c => c.Guid());
            CreateIndex("dbo.Vehicles", "VehicleType_Id");
            AddForeignKey("dbo.Vehicles", "VehicleType_Id", "dbo.VehicleTypes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Vehicles", "VehicleType_Id", "dbo.VehicleTypes");
            DropIndex("dbo.Vehicles", new[] { "VehicleType_Id" });
            DropColumn("dbo.Vehicles", "VehicleType_Id");
            DropTable("dbo.VehicleTypes");
        }
    }
}
