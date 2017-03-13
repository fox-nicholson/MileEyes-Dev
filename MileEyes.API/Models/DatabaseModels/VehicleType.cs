using System;
using System.Collections.Generic;

namespace MileEyes.API.Models.DatabaseModels
{
    public class VehicleType
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new HashSet<Vehicle>();
    }
}