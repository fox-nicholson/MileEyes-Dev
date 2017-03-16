using System;
using System.Collections.Generic;
using MileEyes.PublicModels.Vehicles;

namespace MileEyes.PublicModels.Driver
{
    public class DriverViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
		public bool AutoAccept { get; set; }

        public IList<VehicleViewModel> Vehicles { get; set; }
    }
}