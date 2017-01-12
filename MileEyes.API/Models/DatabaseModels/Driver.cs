using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Models.DatabaseModels
{
    public class Driver : Profile
    {
        public virtual Guid LastActiveVehicle { get; set; }

        public virtual ICollection<Vehicle> Vehicles { get; set; } = new HashSet<Vehicle>();
        public virtual ICollection<Journey> Journeys { get; set; } = new HashSet<Journey>();
    }
}