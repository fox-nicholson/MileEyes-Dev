using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using MileEyes;
using MileEyes.CustomControls;
using MileEyes.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomTabbedPage), typeof(CustomTabbedPageRenderer))]
namespace MileEyes.iOS.Renderers
{
    class CustomTabbedPageRenderer : TabbedRenderer
    {
        IPageController PageController => Element as IPageController;
        IElementController ElementController => Element as IElementController;

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            //this.TabBar.BackgroundImage = new UIImage("Clear.png");
            this.TabBar.TintColor = new UIColor(0.19f, 1.00f, 0.00f, 1.00f);
            //this.TabBar.BackgroundColor = new UIColor(0.06f, 0.25f, 0.28f, 0.5f);
            //this.View.BackgroundColor = UIColor.Clear;
            //this.TabBar.ShadowImage = new UIImage("BorderImage.png");
            

            //var tabBarBlurBGView = new UIVisualEffectView(UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark));
            //tabBarBlurBGView.Frame = this.TabBar.Bounds;
            //this.TabBar.InsertSubview(tabBarBlurBGView, 0);

            var tabs = Element as TabbedPage;
            if (tabs != null)
            {
                for (int i = 0; i < TabBar.Items.Length; i++)
                {
                    UpdateItem(TabBar.Items[i], tabs.Children[i].Icon);
                }
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            //PageController.ContainerArea = new Rectangle(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
        }

        void UpdateItem(UITabBarItem item, string icon)
        {
            if (item == null)
                return;
            try
            {
                icon = icon.Replace("nofill.png", "filled.png");
                if (item?.SelectedImage?.AccessibilityIdentifier == icon)
                    return;
                item.SelectedImage = UIImage.FromBundle(icon);
                item.SelectedImage.AccessibilityIdentifier = icon;

                item.SetTitleTextAttributes(new UITextAttributes() { TextColor = UIColor.Green }, UIControlState.Selected);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to set selected icon: " + ex);
            }
        }
    }
}
