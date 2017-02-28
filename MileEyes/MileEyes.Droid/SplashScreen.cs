using Android.App;
using Android.OS;

namespace MileEyes.Droid
{
    [Activity(Label = "MileEyes", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true,
        Icon = "@drawable/ic_launcher_APP")]
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            //Start Main Activity  
            StartActivity(typeof(MainActivity));
        }
    }
}