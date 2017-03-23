namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeCompanyObjects : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Companies", "AutoAccept");
            DropColumn("dbo.Companies", "Vat");
            DropColumn("dbo.Companies", "VatNumber");
            DropColumn("dbo.Companies", "Personal");
            DropColumn("dbo.Companies", "AutoAcceptDistance");
            DropColumn("dbo.Companies", "StartOfTaxYear");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Companies", "StartOfTaxYear", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Companies", "AutoAcceptDistance", c => c.Double());
            AddColumn("dbo.Companies", "Personal", c => c.Boolean(nullable: false));
            AddColumn("dbo.Companies", "VatNumber", c => c.String());
            AddColumn("dbo.Companies", "Vat", c => c.Boolean(nullable: false));
            AddColumn("dbo.Companies", "AutoAccept", c => c.Boolean(nullable: false));
        }
    }
}
