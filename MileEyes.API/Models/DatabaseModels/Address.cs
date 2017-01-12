using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class Address
    {
        public Guid Id { get; set; }
        public string PlaceId { get; set; }

        public virtual Coordinates Coordinates { get; set; }

        public virtual ICollection<Company> Companies { get; set; } = new HashSet<Company>();
        public virtual ICollection<Waypoint> Waypoints { get; set; } = new HashSet<Waypoint>();
    }
}