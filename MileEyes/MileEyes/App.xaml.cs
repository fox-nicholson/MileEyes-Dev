using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MileEyes.CustomControls;
using MileEyes.Pages;
using Xamarin.Forms;

namespace MileEyes
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Device.OnPlatform(
                iOS: () =>
                {
                    MainPage = new MainPage();
                },
                Android: () =>
                {
                    MainPage = new AndroidMainPage();
                });

            SyncTimer();
            Device.StartTimer(TimeSpan.FromMinutes(1), SyncTimer);
        }

        private bool SyncTimer()
        {
            if (Services.Host.AuthService.Authenticated)
            {
                Services.Host.EngineTypeService.Sync();
                Services.Host.VehicleService.Sync();
                Services.Host.CompanyService.Sync();
            }
            
            return true;
        }

        protected override void OnStart()
        {
            Services.Host.Backgrounded = false;
        }

        protected override void OnSleep()
        {
            Services.Host.Backgrounded = true;
        }

        protected override void OnResume()
        {
            Services.Host.Backgrounded = false;
        }
    }
}
