using System;
using Xamarin.Forms;

namespace MileEyes
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            /*
             * Platform Specific Layouts
             * 
             * Almost Identical apart from Tab Icons being omitted from Android
             */
            Device.OnPlatform(
                iOS: () => { MainPage = new MainPage(); },
                Android: () => { MainPage = new AndroidMainPage(); });

//            SyncTimer();
            Device.StartTimer(TimeSpan.FromMinutes(1), SyncTimer);
        }

        /// <summary>
        /// Triggers Sync of Data for Authenticated Users
        /// </summary>
        /// <returns></returns>
        private static bool SyncTimer()
        {
            if (!Services.Host.AuthService.Authenticated) return true;
            Services.Host.EngineTypeService.Sync();
            Services.Host.VehicleTypeService.Sync();
            Services.Host.VehicleService.Sync();
            Services.Host.CompanyService.Sync();
            Services.Host.JourneyService.Sync();

            return true;
        }

        /// <summary>
        /// Sets Backgrounded to false because App has started up (self explanitory)
        /// </summary>
        protected override void OnStart()
        {
            Services.Host.Backgrounded = false;
        }

        /// <summary>
        /// Sets Backgrounded to true because App has gone into the background (self explanitory)
        /// </summary>
        protected override void OnSleep()
        {
            Services.Host.Backgrounded = true;
        }

        /// <summary>
        /// Sets Backgrounded to false because App has come into focus (self explanitory)
        /// </summary>
        protected override void OnResume()
        {
            Services.Host.Backgrounded = false;
        }
    }
}