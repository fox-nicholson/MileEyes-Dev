using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels.EngineTypes;

namespace MileEyes.PublicModels.Vehicles
{
    public class VehicleViewModel
    {
        public string Id { get; set; }
        public string Registration { get; set; }
        public EngineTypeViewModel EngineType { get; set; }
    }
}
