using MileEyes.PublicModels.EngineTypes;
using MileEyes.PublicModels.VehicleTypes;
using System;

namespace MileEyes.PublicModels.Vehicles
{
    public class VehicleViewModel
    {
        public string Id { get; set; }
        public string Registration { get; set; }
        public EngineTypeViewModel EngineType { get; set; }
        public VehicleTypeViewModel VehicleType { get; set; }
    }
}