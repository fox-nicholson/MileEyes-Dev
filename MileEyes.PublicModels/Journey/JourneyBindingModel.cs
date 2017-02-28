using System;
using System.Collections.Generic;
using MileEyes.PublicModels.Company;
using MileEyes.PublicModels.Vehicles;

namespace MileEyes.PublicModels.Journey
{
    public class JourneyBindingModel
    {
        public string Reason { get; set; }
        public bool Invoiced { get; set; }
        public int Passengers { get; set; }
        public double Distance { get; set; }
        public DateTimeOffset Date { get; set; }
        public CompanyBindingModel Company { get; set; }
        public VehicleBindingModel Vehicle { get; set; }
        public IList<WaypointBindingModel> Waypoints { get; set; }
    }
}