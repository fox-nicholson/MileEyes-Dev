using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MileEyes.Services.Helpers
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

        public static double[] MidPoint(double[] posA, double[] posB)
        {
            double[] midPoint = new double[] { 0, 0 };

            double dLon = Deg2Rad(posB[1] - posA[1]);
            double Bx = Math.Cos(Deg2Rad(posB[0])) * Math.Cos(dLon);
            double By = Math.Cos(Deg2Rad(posB[0])) * Math.Sin(dLon);

            var lat = Rad2Deg(Math.Atan2(
                         Math.Sin(Deg2Rad(posA[0])) + Math.Sin(Deg2Rad(posB[0])),
                         Math.Sqrt(
                             (Math.Cos(Deg2Rad(posA[0])) + Bx) *
                             (Math.Cos(Deg2Rad(posA[0])) + Bx) + By * By)));
            // (Math.Cos(DegreesToRadians(posA.Latitude))) + Bx) + By * By)); // Your Code

            var lng = posA[1] + Rad2Deg(Math.Atan2(By, Math.Cos(Deg2Rad(posA[0])) + Bx));
            midPoint[0] = lat;
            midPoint[1] = lng;
            return midPoint;
        }
    }
}
