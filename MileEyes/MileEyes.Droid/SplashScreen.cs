using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MileEyes.Droid
{
    [Activity(Label = "MileEyes", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true,
        Icon = "@drawable/mileeyes_icon")]
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