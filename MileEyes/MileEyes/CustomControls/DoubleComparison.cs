using System;

namespace MileEyes.CustomControls
{
    internal class DoubleComparison
    {
        public static bool isEqual(double double1, double double2)
        {
            // Define the tolerance for variation in their values
            var difference = Math.Abs(double1 * .00001);

            // Compare the values

            return Math.Abs(double1 - double2) <= difference;
            // The two values are inequal
        }
    }
}