namespace MileEyes.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Profiles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LastActiveCompany = c.Guid(nullable: false),
                        LastActiveVehicle = c.Guid(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.AccountingTokens",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Token = c.String(),
                        UserId = c.String(),
                        OrganisationId = c.String(),
                        ConsumerKey = c.String(),
                        ConsumerSecret = c.String(),
                        TokenKey = c.String(),
                        TokenSecret = c.String(),
                        ExpiresAt = c.DateTimeOffset(precision: 7),
                        Session = c.String(),
                        SessionExpiresAt = c.DateTimeOffset(precision: 7),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Profile_Id = c.Guid(),
                        Company_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Profiles", t => t.Profile_Id)
                .ForeignKey("dbo.Companies", t => t.Company_Id)
                .Index(t => t.Profile_Id)
                .Index(t => t.Company_Id);
            
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        HighRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LowRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AutoAccept = c.Boolean(nullable: false),
                        Vat = c.Boolean(nullable: false),
                        VatNumber = c.String(),
                        Personal = c.Boolean(nullable: false),
                        AutoAcceptDistance = c.Double(),
                        Modified = c.DateTimeOffset(nullable: false, precision: 7),
                        SubscriptionId = c.String(),
                        Address_Id = c.Guid(),
                        Currency_Id = c.Guid(),
                        Owner_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Addresses", t => t.Address_Id)
                .ForeignKey("dbo.CurrencyRates", t => t.Currency_Id)
                .ForeignKey("dbo.Profiles", t => t.Owner_Id)
                .Index(t => t.Address_Id)
                .Index(t => t.Currency_Id)
                .Index(t => t.Owner_Id);
            
            CreateTable(
                "dbo.AccountingEntries",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        EntryId = c.String(),
                        Company_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.Company_Id)
                .Index(t => t.Company_Id);
            
            CreateTable(
                "dbo.Journeys",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Modified = c.DateTimeOffset(nullable: false, precision: 7),
                        Reason = c.String(),
                        Distance = c.Double(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FuelVat = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Invoiced = c.Boolean(nullable: false),
                        Passengers = c.Int(nullable: false),
                        Date = c.DateTimeOffset(nullable: false, precision: 7),
                        Accepted = c.Boolean(nullable: false),
                        Rejected = c.Boolean(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        AccountingEntry_Id = c.Guid(),
                        Company_Id = c.Guid(),
                        Driver_Id = c.Guid(),
                        Vehicle_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AccountingEntries", t => t.AccountingEntry_Id)
                .ForeignKey("dbo.Companies", t => t.Company_Id)
                .ForeignKey("dbo.Profiles", t => t.Driver_Id)
                .ForeignKey("dbo.Vehicles", t => t.Vehicle_Id)
                .Index(t => t.AccountingEntry_Id)
                .Index(t => t.Company_Id)
                .Index(t => t.Driver_Id)
                .Index(t => t.Vehicle_Id);
            
            CreateTable(
                "dbo.Adjustments",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Distance = c.Double(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        Company_Id = c.Guid(),
                        Profile_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.Company_Id)
                .ForeignKey("dbo.Profiles", t => t.Profile_Id)
                .Index(t => t.Company_Id)
                .Index(t => t.Profile_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        CustomerId = c.String(),
                        SignupDate = c.DateTimeOffset(nullable: false, precision: 7),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                        Address_Id = c.Guid(),
                        Currency_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Addresses", t => t.Address_Id)
                .ForeignKey("dbo.CurrencyRates", t => t.Currency_Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex")
                .Index(t => t.Address_Id)
                .Index(t => t.Currency_Id);
            
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PlaceId = c.String(),
                        Coordinates_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Coordinates", t => t.Coordinates_Id)
                .Index(t => t.Coordinates_Id);
            
            CreateTable(
                "dbo.Coordinates",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Waypoints",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        Step = c.Int(nullable: false),
                        Address_Id = c.Guid(),
                        Journey_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Addresses", t => t.Address_Id)
                .ForeignKey("dbo.Journeys", t => t.Journey_Id)
                .Index(t => t.Address_Id)
                .Index(t => t.Journey_Id);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.CurrencyRates",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Code = c.String(),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Vehicles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Registration = c.String(),
                        Modified = c.DateTimeOffset(nullable: false, precision: 7),
                        EngineType_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EngineTypes", t => t.EngineType_Id)
                .Index(t => t.EngineType_Id);
            
            CreateTable(
                "dbo.EngineTypes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Size = c.Int(nullable: false),
                        FuelRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Name = c.String(),
                        FuelType_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FuelTypes", t => t.FuelType_Id)
                .Index(t => t.FuelType_Id);
            
            CreateTable(
                "dbo.FuelTypes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Invites",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        Email = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Company_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.Company_Id)
                .Index(t => t.Company_Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.ProfileCompanies",
                c => new
                    {
                        Profile_Id = c.Guid(nullable: false),
                        Company_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Profile_Id, t.Company_Id })
                .ForeignKey("dbo.Profiles", t => t.Profile_Id, cascadeDelete: true)
                .ForeignKey("dbo.Companies", t => t.Company_Id, cascadeDelete: true)
                .Index(t => t.Profile_Id)
                .Index(t => t.Company_Id);
            
            CreateTable(
                "dbo.VehicleDrivers",
                c => new
                    {
                        Vehicle_Id = c.Guid(nullable: false),
                        Driver_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Vehicle_Id, t.Driver_Id })
                .ForeignKey("dbo.Vehicles", t => t.Vehicle_Id, cascadeDelete: true)
                .ForeignKey("dbo.Profiles", t => t.Driver_Id, cascadeDelete: true)
                .Index(t => t.Vehicle_Id)
                .Index(t => t.Driver_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Companies", "Owner_Id", "dbo.Profiles");
            DropForeignKey("dbo.Invites", "Company_Id", "dbo.Companies");
            DropForeignKey("dbo.Companies", "Currency_Id", "dbo.CurrencyRates");
            DropForeignKey("dbo.AccountingTokens", "Company_Id", "dbo.Companies");
            DropForeignKey("dbo.Journeys", "Vehicle_Id", "dbo.Vehicles");
            DropForeignKey("dbo.Vehicles", "EngineType_Id", "dbo.EngineTypes");
            DropForeignKey("dbo.EngineTypes", "FuelType_Id", "dbo.FuelTypes");
            DropForeignKey("dbo.VehicleDrivers", "Driver_Id", "dbo.Profiles");
            DropForeignKey("dbo.VehicleDrivers", "Vehicle_Id", "dbo.Vehicles");
            DropForeignKey("dbo.Journeys", "Driver_Id", "dbo.Profiles");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Profiles", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "Currency_Id", "dbo.CurrencyRates");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "Address_Id", "dbo.Addresses");
            DropForeignKey("dbo.Waypoints", "Journey_Id", "dbo.Journeys");
            DropForeignKey("dbo.Waypoints", "Address_Id", "dbo.Addresses");
            DropForeignKey("dbo.Addresses", "Coordinates_Id", "dbo.Coordinates");
            DropForeignKey("dbo.Companies", "Address_Id", "dbo.Addresses");
            DropForeignKey("dbo.ProfileCompanies", "Company_Id", "dbo.Companies");
            DropForeignKey("dbo.ProfileCompanies", "Profile_Id", "dbo.Profiles");
            DropForeignKey("dbo.Adjustments", "Profile_Id", "dbo.Profiles");
            DropForeignKey("dbo.AccountingTokens", "Profile_Id", "dbo.Profiles");
            DropForeignKey("dbo.Adjustments", "Company_Id", "dbo.Companies");
            DropForeignKey("dbo.Journeys", "Company_Id", "dbo.Companies");
            DropForeignKey("dbo.Journeys", "AccountingEntry_Id", "dbo.AccountingEntries");
            DropForeignKey("dbo.AccountingEntries", "Company_Id", "dbo.Companies");
            DropIndex("dbo.VehicleDrivers", new[] { "Driver_Id" });
            DropIndex("dbo.VehicleDrivers", new[] { "Vehicle_Id" });
            DropIndex("dbo.ProfileCompanies", new[] { "Company_Id" });
            DropIndex("dbo.ProfileCompanies", new[] { "Profile_Id" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Invites", new[] { "Company_Id" });
            DropIndex("dbo.EngineTypes", new[] { "FuelType_Id" });
            DropIndex("dbo.Vehicles", new[] { "EngineType_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.Waypoints", new[] { "Journey_Id" });
            DropIndex("dbo.Waypoints", new[] { "Address_Id" });
            DropIndex("dbo.Addresses", new[] { "Coordinates_Id" });
            DropIndex("dbo.AspNetUsers", new[] { "Currency_Id" });
            DropIndex("dbo.AspNetUsers", new[] { "Address_Id" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Adjustments", new[] { "Profile_Id" });
            DropIndex("dbo.Adjustments", new[] { "Company_Id" });
            DropIndex("dbo.Journeys", new[] { "Vehicle_Id" });
            DropIndex("dbo.Journeys", new[] { "Driver_Id" });
            DropIndex("dbo.Journeys", new[] { "Company_Id" });
            DropIndex("dbo.Journeys", new[] { "AccountingEntry_Id" });
            DropIndex("dbo.AccountingEntries", new[] { "Company_Id" });
            DropIndex("dbo.Companies", new[] { "Owner_Id" });
            DropIndex("dbo.Companies", new[] { "Currency_Id" });
            DropIndex("dbo.Companies", new[] { "Address_Id" });
            DropIndex("dbo.AccountingTokens", new[] { "Company_Id" });
            DropIndex("dbo.AccountingTokens", new[] { "Profile_Id" });
            DropIndex("dbo.Profiles", new[] { "User_Id" });
            DropTable("dbo.VehicleDrivers");
            DropTable("dbo.ProfileCompanies");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Invites");
            DropTable("dbo.FuelTypes");
            DropTable("dbo.EngineTypes");
            DropTable("dbo.Vehicles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.CurrencyRates");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.Waypoints");
            DropTable("dbo.Coordinates");
            DropTable("dbo.Addresses");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Adjustments");
            DropTable("dbo.Journeys");
            DropTable("dbo.AccountingEntries");
            DropTable("dbo.Companies");
            DropTable("dbo.AccountingTokens");
            DropTable("dbo.Profiles");
        }
    }
}
