namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeUserAddress : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "Address_Id", "dbo.Addresses");
            DropIndex("dbo.AspNetUsers", new[] { "Address_Id" });
            DropColumn("dbo.AspNetUsers", "Address_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Address_Id", c => c.Guid());
            CreateIndex("dbo.AspNetUsers", "Address_Id");
            AddForeignKey("dbo.AspNetUsers", "Address_Id", "dbo.Addresses", "Id");
        }
    }
}
