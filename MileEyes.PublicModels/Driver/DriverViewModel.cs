using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.PublicModels.Vehicles;

namespace MileEyes.PublicModels.Driver
{
    public class DriverViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

		public Guid LastActiveVehicle { get; set; }

		public IList<VehicleViewModel> Vehicles { get; set; }
    }
}
