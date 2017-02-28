using System;
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
        IElementController ElementController => Element;

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            TabBar.TintColor = new UIColor(0.19f, 1.00f, 0.00f, 1.00f);
            
            var tabs = Element as TabbedPage;
            if (tabs == null) return;

            for (var i = 0; i < TabBar.Items.Length; i++)
            {
                UpdateItem(TabBar.Items[i], tabs.Children[i].Icon);
            }
        }

        private static void UpdateItem(UITabBarItem item, string icon)
        {
            if (item == null)
                return;
            try
            {
                icon = icon.Replace("nofill.png", "filled.png");
                if (item.SelectedImage?.AccessibilityIdentifier == icon)
                    return;
                item.SelectedImage = UIImage.FromBundle(icon);
                item.SelectedImage.AccessibilityIdentifier = icon;

                item.SetTitleTextAttributes(new UITextAttributes() {TextColor = UIColor.Green}, UIControlState.Selected);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to set selected icon: " + ex);
            }
        }
    }
}