using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MileEyes.Services
{
    public class Host
    {
        public static bool Backgrounded { get; set; }

        public static IAuthService AuthService = new AuthService();
        
        public static IUserService UserService = new UserService();

        public static IVehicleService VehicleService = new VehicleService();
        public static IEngineTypeService EngineTypeService = new EngineTypeService();
        public static ICompanyService CompanyService = new CompanyService();
        public static IJourneyService JourneyService = new JourneyService();

        public static IReasonService ReasonService = new ReasonService();

        public static ITrackerService TrackerService = new TrackerService();

        public static IGeocodingService GeocodingService = new GeocodingService();

        public static IHttpHelper HttpHelper = new HttpHelper();
    }
}
