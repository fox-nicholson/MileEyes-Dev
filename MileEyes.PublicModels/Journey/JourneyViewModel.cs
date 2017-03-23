using System;
using System.Collections.Generic;
using MileEyes.PublicModels.Company;
using MileEyes.PublicModels.Driver;
using MileEyes.PublicModels.Vehicles;

namespace MileEyes.PublicModels.Journey
{
    public class JourneyViewModel
    {
        public string Id { get; set; }
        public string Reason { get; set; }
        public bool Invoiced { get; set; }
        public int Passengers { get; set; }
        public double Distance { get; set; }
        public double Cost { get; set; }
        public DateTimeOffset Date { get; set; }
        public CompanyViewModel Company { get; set; }
        public VehicleViewModel Vehicle { get; set; }
        public int Waypoints { get; set; }
        public DateTimeOffset Modified { get; set; }

        public DriverViewModel Driver { get; set; }

        public bool Accepted { get; set; }
        public bool Rejected { get; set; }
    }
}