using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class Company
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public decimal HighRate { get; set; }
        public decimal LowRate { get; set; }

        public virtual Owner Owner { get; set; }
        
        public virtual CurrencyRate Currency { get; set; }

        public virtual ICollection<Profile> Profiles { get; set; } = new HashSet<Profile>();

        public virtual ICollection<Adjustment> Adjustments { get; set; } = new HashSet<Adjustment>();
        public virtual ICollection<Journey> Journeys { get; set; } = new HashSet<Journey>();
        public virtual ICollection<AccountingToken> AccountingTokens { get; set; } = new HashSet<AccountingToken>();
        public virtual ICollection<AccountingEntry> AccountingEntries { get; set; } = new HashSet<AccountingEntry>();
        public virtual ICollection<DriverInvite> DriverInvites { get; set; } = new HashSet<DriverInvite>();

        public DateTimeOffset Modified { get; set; }

        public string SubscriptionId { get; set; }
    }
}