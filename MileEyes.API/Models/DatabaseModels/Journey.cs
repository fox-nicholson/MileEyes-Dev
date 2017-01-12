using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class Journey
    {
        public Guid Id { get; set; }

        public DateTimeOffset Modified { get; set; }

        public string Reason { get; set; }

        /// <summary>
        /// !!! MEGA FUCKING IMPORTANT !!!
        /// !!! DISTANCES STORED IN METERS !!!
        /// </summary>
        public double Distance { get; set; }

        public decimal Cost { get; set; }
        public decimal FuelVat { get; set; }
        public bool Invoiced { get; set; }
        public int Passengers { get; set; }
        public DateTimeOffset Date { get; set; }
        public bool Accepted { get; set; }
        public bool Rejected { get; set; }

        public bool Deleted { get; set; }

        public virtual Driver Driver { get; set; }
        public virtual Company Company { get; set; }
        public virtual Vehicle Vehicle { get; set; }

        public virtual ICollection<Waypoint> Waypoints { get; set; } = new HashSet<Waypoint>();
        public virtual AccountingEntry AccountingEntry { get; set; }
    }
}