namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeregdate : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.VehicleTypes", "RegDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.VehicleTypes", "RegDate", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
    }
}
