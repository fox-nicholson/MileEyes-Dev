using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MileEyes.Services.Models
{
    public class Address
    {
        public string PlaceId { get; set; }
        public string Label { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
