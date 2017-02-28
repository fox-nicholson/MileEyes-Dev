using MileEyes.PublicModels.EngineTypes;

namespace MileEyes.PublicModels.Vehicles
{
    public class VehicleBindingModel
    {
        public string Id { get; set; }
        public string CloudId { get; set; }
        public string Registration { get; set; }
        public EngineTypeBindingModel EngineType { get; set; }
    }
}