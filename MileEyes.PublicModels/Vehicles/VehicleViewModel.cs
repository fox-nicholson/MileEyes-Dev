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