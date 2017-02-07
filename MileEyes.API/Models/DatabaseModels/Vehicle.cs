using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class Vehicle
    {
        public Guid Id { get; set; }
    
        public string Registration { get; set; }

        public DateTimeOffset Modified { get; set; }

        public virtual ICollection<Driver> Drivers { get; set; } = new HashSet<Driver>();
        public virtual ICollection<Journey> Journeys { get; set; } = new HashSet<Journey>();
        public virtual EngineType EngineType { get; set; }
    }
}