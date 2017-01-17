namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AvatarImage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "AvatarImage", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "AvatarImage");
        }
    }
}
