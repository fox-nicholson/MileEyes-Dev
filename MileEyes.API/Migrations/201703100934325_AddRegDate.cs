namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRegDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Vehicles", "RegDate", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Vehicles", "RegDate");
        }
    }
}
