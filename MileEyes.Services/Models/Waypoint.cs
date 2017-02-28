using System;
using Realms;

namespace MileEyes.Services.Models
{
    public class Waypoint : RealmObject
    {
        public string Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Label { get; set; }
        public int Step { get; set; }
        public string PlaceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}