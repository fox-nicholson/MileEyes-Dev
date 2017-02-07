using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using MileEyes.API.Models.DatabaseModels;

namespace MileEyes.API.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public byte[] AvatarImage { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CustomerId { get; set; }

        public virtual Address Address { get; set; }

        public DateTimeOffset SignupDate { get; set; }

        public virtual CurrencyRate Currency { get; set; }

        public virtual ICollection<Profile> Profiles { get; set; } = new HashSet<Profile>();
        
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {

        }
        
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<CurrencyRate> CurrencyRates { get; set; }

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Accountant> Accountants { get; set; }
        public DbSet<Driver> Drivers { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Journey> Journeys { get; set; }
        public DbSet<Waypoint> Waypoints { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Coordinates> Coordinates { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<EngineType> EngineTypes { get; set; }
        public DbSet<FuelType> FuelTypes { get; set; }

        public DbSet<AccountingEntry> AccountingEntries { get; set; }

        public DbSet<AccountingToken> AccountingTokens { get; set; }
        public DbSet<XeroToken> XeroTokens { get; set; }

        public DbSet<Adjustment> Adjustments { get; set; }

        public DbSet<Invite> Invites { get; set; }
        public DbSet<DriverInvite> DriverInvites { get; set; }
    }
}