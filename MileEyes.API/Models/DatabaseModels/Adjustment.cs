using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class Adjustment
    {
        public Guid Id { get; set; }

        public double Distance { get; set; }
        public decimal Cost { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public virtual Company Company { get; set; }

        public virtual Profile Profile { get; set; }
    }
}