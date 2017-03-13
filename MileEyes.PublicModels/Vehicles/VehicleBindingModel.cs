using MileEyes.PublicModels.EngineTypes;
using MileEyes.PublicModels.VehicleTypes;
using System;

namespace MileEyes.PublicModels.Vehicles
{
    public class VehicleBindingModel
    {
        public string Id { get; set; }
        public string CloudId { get; set; }
        public string Registration { get; set; }
        public DateTime RegDate { get; set; }
        public EngineTypeBindingModel EngineType { get; set; }
        public VehicleTypeBindingModel VehicleType { get; set; }
    }
}