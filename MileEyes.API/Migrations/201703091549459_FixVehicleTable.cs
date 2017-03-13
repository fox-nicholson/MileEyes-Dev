namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixVehicleTable : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Vehicles", "RegDate");
            DropColumn("dbo.Vehicles", "VehicleType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Vehicles", "VehicleType", c => c.String());
            AddColumn("dbo.Vehicles", "RegDate", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
    }
}
