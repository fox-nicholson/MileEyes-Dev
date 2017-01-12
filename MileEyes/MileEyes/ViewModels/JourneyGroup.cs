using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MileEyes.ViewModels
{
    class JourneyGroup : List<JourneyViewModel>
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
