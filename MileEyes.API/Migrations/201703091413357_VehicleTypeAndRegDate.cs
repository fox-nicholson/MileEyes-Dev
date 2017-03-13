namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VehicleTypeAndRegDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Vehicles", "RegDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Vehicles", "VehicleType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Vehicles", "VehicleType");
            DropColumn("dbo.Vehicles", "RegDate");
        }
    }
}
