using System;

namespace MileEyes.PublicModels.Journey
{
    public class WaypointViewModel
    {
        public string Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int Step { get; set; }
        public string PlaceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}