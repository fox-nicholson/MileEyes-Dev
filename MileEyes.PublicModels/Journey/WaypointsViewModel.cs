using System.Collections.Generic;

namespace MileEyes.PublicModels.Journey
{
    public class WaypointsViewModel
    {
        public IList<WaypointViewModel> Waypoints { get; set; }
        public int first { get; set; }
        public int last { get; set; }
        public string journeyId { get; set; }
    }
}
