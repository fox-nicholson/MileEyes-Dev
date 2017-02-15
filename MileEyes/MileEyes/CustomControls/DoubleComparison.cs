using System;

namespace MileEyes.CustomControls
{
    class DoubleComparison
    {
        public static bool isEqual(double double1, double double2)
        {
            // Define the tolerance for variation in their values
            double difference = Math.Abs(double1 * .00001);

            // Compare the values
            
            if (Math.Abs(double1 - double2) <= difference)
                // The two values are equal
                return true;
            // The two values are inequal
                return false;
        }
    }
}
