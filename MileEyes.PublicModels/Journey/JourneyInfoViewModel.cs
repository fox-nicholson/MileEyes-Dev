using System;
using System.Collections.Generic;
using MileEyes.PublicModels.Company;
using MileEyes.PublicModels.Driver;
using MileEyes.PublicModels.Vehicles;

namespace MileEyes.PublicModels.Journey
{
    public class JourneyInfoViewModel
    {
        public Guid Id { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}