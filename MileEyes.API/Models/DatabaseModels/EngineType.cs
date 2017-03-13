using System;
using System.Collections.Generic;

namespace MileEyes.API.Models.DatabaseModels
{
    public class EngineType
    {
        public Guid Id { get; set; }

        public int Size { get; set; }
        public decimal FuelRate { get; set; }
        public string Name { get; set; }
        
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new HashSet<Vehicle>();
        public virtual FuelType FuelType { get; set; }
    }
}