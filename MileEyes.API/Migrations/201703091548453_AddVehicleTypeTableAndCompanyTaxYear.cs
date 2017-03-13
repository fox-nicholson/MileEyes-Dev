namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVehicleTypeTableAndCompanyTaxYear : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "StartOfTaxYear", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "StartOfTaxYear");
        }
    }
}
