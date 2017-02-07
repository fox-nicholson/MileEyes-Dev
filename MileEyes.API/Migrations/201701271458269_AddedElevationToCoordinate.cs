namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedElevationToCoordinate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Coordinates", "Elevation", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Coordinates", "Elevation");
        }
    }
}
