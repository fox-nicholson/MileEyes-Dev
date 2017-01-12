using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MileEyes.API.Helpers
{
    public class TrigHelpers
    {
        public static double Deg2Rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        public static double Rad2Deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
    }
}