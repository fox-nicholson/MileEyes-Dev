using System;
using System.Collections.Generic;
using System.Text;
using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomNavigationPage), typeof(CustomNavigationPageRenderer))]
namespace MileEyes.iOS.Renderers
{
    class CustomNavigationPageRenderer : NavigationRenderer
    {
        IPageController PageController => Element as IPageController;
        IElementController ElementController => Element as IElementController;

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            //UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);
            //UIApplication.SharedApplication.SetStatusBarHidden(false, false);

            //this.NavigationBar.BarStyle = UIBarStyle.BlackTranslucent;
            //this.NavigationBar.Translucent = true;
            //this.NavigationBar.TintColor = new UIColor(0.19f, 1.00f, 0.00f, 1.00f);
            //this.NavigationBar.BackgroundColor = new UIColor(0.06f, 0.25f, 0.28f, 0.5f);
            //this.NavigationBar.ShadowImage = new UIImage("BorderImage.png");
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            //if (this.ChildViewControllers != null)
            //{
            //    var childviews = this.ChildViewControllers;

            //    foreach (var c in childviews)
            //    {
            //        c.EdgesForExtendedLayout = UIRectEdge.All;
            //        c.AutomaticallyAdjustsScrollViewInsets = true;
            //    }
            //}
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            //PageController.ContainerArea = new Rectangle(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
        }
    }
}
