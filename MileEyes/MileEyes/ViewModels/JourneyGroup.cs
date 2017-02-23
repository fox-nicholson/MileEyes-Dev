using System.Collections.Generic;

namespace MileEyes.ViewModels
{
    internal class JourneyGroup : List<JourneyViewModel>
    {
        public string Title { get; set; }
        public string ShortName { get; set; } //will be used for jump lists
        public string Subtitle { get; set; }

        public JourneyGroup(string title, string shortName)
        {
            Title = title;
            ShortName = shortName;
        }
    }
}