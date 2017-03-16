using System.Collections.Generic;

namespace MileEyes.PublicModels.Journey
{
    public class WaypointsBindingModel
    {
        public IList<WaypointBindingModel> Waypoints { get; set; }
        public int first { get; set; }
        public int last { get; set; }
        public JourneyBindingModel Journey { get; set; }
        public string journeyId { get; set; }
    }
}
