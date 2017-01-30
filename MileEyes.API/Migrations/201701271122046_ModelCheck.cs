namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModelCheck : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AccountingTokens", "access_token", c => c.String());
            AddColumn("dbo.AccountingTokens", "token_type", c => c.String());
            AddColumn("dbo.AccountingTokens", "expires_in", c => c.Int());
            AddColumn("dbo.AccountingTokens", "refresh_token", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AccountingTokens", "refresh_token");
            DropColumn("dbo.AccountingTokens", "expires_in");
            DropColumn("dbo.AccountingTokens", "token_type");
            DropColumn("dbo.AccountingTokens", "access_token");
        }
    }
}
