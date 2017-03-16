namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSignupDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PendingUsers", "SignUpDate", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PendingUsers", "SignUpDate");
        }
    }
}
