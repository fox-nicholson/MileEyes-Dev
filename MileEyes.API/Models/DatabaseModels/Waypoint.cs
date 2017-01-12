using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class Waypoint
    {
        public Guid Id { get; set; }

        public DateTimeOffset Timestamp { get; set; }
        public int Step { get; set; }

        public virtual Address Address { get; set; }
        public virtual Journey Journey { get; set; }
    }
}