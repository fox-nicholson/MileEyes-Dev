using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels.EngineTypes;

namespace MileEyes.PublicModels.Vehicles
{
    public class VehicleBindingModel
    {
        public string Id { get; set; }
        public string Registration { get; set; }
        public EngineTypeBindingModel EngineType { get; set; }
    }
}
