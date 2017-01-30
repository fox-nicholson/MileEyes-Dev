using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using MileEyes.Services.Models;
using UIKit;
using Xamarin.Forms;

namespace MileEyes.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            global::Xamarin.FormsMaps.Init();
            LoadApplication(new App());

            MessagingCenter.Subscribe<SharedJourney>(this, "Share", Share, null);

            return base.FinishedLaunching(app, options);
        }

        private void Share(SharedJourney journey)
        {
            //var item = new Models.SharedJourney()
            //{
            //    .From = journey.From,
            //    .To = journey.To,
            //    .Reason = journey.Reason,
            //    Company = journey.Company,
            //    Cost = journey.Cost,
            //    .Date = journey.Date,
            //    .Distance = journey.Distance,
            //    FuelVat = journey.FuelVat,
            //    .Invoiced = journey.Invoiced,
            //    .Passengers = journey.Passengers,
            //    .Vehicle = journey.Vehicle
            //};

            var invoiced = "No";

            if (journey.Invoiced)
            {
                invoiced = "Yes";
            }

            var item = new NSString(
                "MileEyes Mileage Expense Claim" + Environment.NewLine +
                "Date: " + journey.Date + Environment.NewLine +
                "Distance: " + $"{journey.Distance:N2}" + " miles" + Environment.NewLine +
                "From: " + journey.From + Environment.NewLine +
                "To: " + journey.To + Environment.NewLine +
                "Reason: " + journey.Reason + Environment.NewLine +
                "Vehicle: " + journey.Vehicle + Environment.NewLine +
                "Passengers: " + journey.Passengers + Environment.NewLine +
                "Invoiced: " + invoiced);

            var activityItems = new[] { item };
            var activityController = new UIActivityViewController(activityItems, null);

            var topController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            while (topController.PresentedViewController != null)
            {
                topController = topController.PresentedViewController;
            }

            topController.PresentViewController(activityController, true, () => { });
        }
    }
}
