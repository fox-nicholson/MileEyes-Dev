namespace MileEyes.Services.Helpers
{
    public class Units
    {
        public static double MetersToMiles(double meters)
        {
            return meters * 0.00062137;
        }

        public static double MilesToMeters(double miles)
        {
            return miles / 0.00062137;
        }
    }
}