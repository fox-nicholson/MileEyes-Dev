using System;
using System.Collections.Generic;
using Realms;

namespace MileEyes.Services.Models
{
    public class Journey : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string CloudId { get; set; }
        public string Reason { get; set; }
        public bool Invoiced { get; set; }
        public int Passengers { get; set; }
        public double Distance { get; set; }
        public double Cost { get; set; }
        public DateTimeOffset Date { get; set; }
        public Company Company { get; set; }
        public Vehicle Vehicle { get; set; }
        public IList<Waypoint> Waypoints { get; }

        public bool Accepted { get; set; }
        public bool Rejected { get; set; }

        public bool MarkedForDeletion { get; set; }
        public bool Deleted { get; set; }
    }
}