using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class AccountingEntry
    {
        public Guid Id { get; set; }
        
        public string EntryId { get; set; }
        
        public virtual Company Company { get; set; }

        public virtual ICollection<Journey> Journeys { get; set; } = new HashSet<Journey>();
    }
}