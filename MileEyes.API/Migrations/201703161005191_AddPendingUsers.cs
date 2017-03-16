namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPendingUsers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PendingUsers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Email = c.String(),
                        SubscriptionId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PendingUsers");
        }
    }
}
