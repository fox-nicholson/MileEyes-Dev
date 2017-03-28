namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class passwordReset : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PasswordResets",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Email = c.String(),
                        Date = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Invites", "CurrentMileage", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Invites", "CurrentMileage");
            DropTable("dbo.PasswordResets");
        }
    }
}
