namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoreJourneyParams : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Journeys", "UnderDistance", c => c.Double(nullable: false));
            AddColumn("dbo.Journeys", "OverDistance", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Journeys", "OverDistance");
            DropColumn("dbo.Journeys", "UnderDistance");
        }
    }
}
