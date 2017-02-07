using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class Profile
    {
        public Guid Id { get; set; }

        public virtual ApplicationUser User { get; set; }
        
        public virtual Guid LastActiveCompany { get; set; }
        
        public virtual ICollection<Company> Companies { get; set; } = new HashSet<Company>();
        public virtual ICollection<AccountingToken> AccountingTokens { get; set; } = new HashSet<AccountingToken>();
        public virtual ICollection<Adjustment> Adjustments { get; set; } = new HashSet<Adjustment>();
    }
}