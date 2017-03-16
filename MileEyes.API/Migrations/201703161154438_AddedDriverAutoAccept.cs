namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDriverAutoAccept : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DriverInfoes", "AutoAccept", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DriverInfoes", "AutoAccept");
        }
    }
}
